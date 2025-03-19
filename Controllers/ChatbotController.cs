// Controllers/ChatbotController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using vfstyle_backend.Data;
using vfstyle_backend.DTOs;
using vfstyle_backend.Models;
using vfstyle_backend.Services;

namespace vfstyle_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly ILLMService _llmService;
        private readonly ApplicationDbContext _context;

        public ChatbotController(ILLMService llmService, ApplicationDbContext context)
        {
            _llmService = llmService;
            _context = context;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                // Tạo danh sách tin nhắn với một system message và tin nhắn của người dùng
                var messages = new List<(string role, string content)>
                {
                    ("system", "Bạn là trợ lý ảo của VF Style, một cửa hàng bán kính mắt. " +
                              "Nhiệm vụ của bạn là giúp khách hàng tìm kiếm kính mắt phù hợp. " +
                              "Hãy trả lời thân thiện, ngắn gọn và hữu ích bằng tiếng Việt."),
                    ("user", request.Message)
                };

                // Gọi service để lấy phản hồi
                var response = await _llmService.GetChatCompletionAsync(messages);

                // Lưu tin nhắn vào database nếu cần
                if (request.SaveConversation)
                {
                    await SaveConversation(request.Message, response, request.ConversationId);
                }

                return Ok(new { response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Message cannot be empty");
            }

            try
            {
                // Lấy lịch sử hội thoại nếu có conversationId
                var messages = new List<(string role, string content)>
                {
                    ("system", "Bạn là trợ lý ảo của VF Style, một cửa hàng bán kính mắt. " +
                              "Nhiệm vụ của bạn là giúp khách hàng tìm kiếm kính mắt phù hợp. " +
                              "Hãy trả lời thân thiện, ngắn gọn và hữu ích bằng tiếng Việt.")
                };

                if (request.ConversationId.HasValue)
                {
                    var conversation = await _context.Conversations
                        .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                        .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value);

                    if (conversation != null)
                    {
                        foreach (var msg in conversation.Messages)
                        {
                            var role = msg.Sender == "User" ? "user" : "assistant";
                            messages.Add((role, msg.Content));
                        }
                    }
                }

                // Thêm tin nhắn hiện tại của người dùng
                messages.Add(("user", request.Message));

                // Gọi service để lấy phản hồi
                var response = await _llmService.GetChatCompletionAsync(messages);

                // Lưu tin nhắn vào database
                var conversationId = await SaveConversation(request.Message, response, request.ConversationId);

                return Ok(new { response, conversationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<int> SaveConversation(string userMessage, string botResponse, int? conversationId = null)
{
    try
    {
        Conversation conversation;

        if (conversationId.HasValue)
        {
            // Tìm cuộc hội thoại hiện có
            conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId.Value);

            if (conversation == null)
            {
                // Tạo mới nếu không tìm thấy
                conversation = new Conversation
                {
                    CreatedAt = DateTime.UtcNow,
                    Messages = new List<Message>()
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync(); // Lưu trước để có Id
            }
            else
            {
                conversation.UpdatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Tạo cuộc hội thoại mới
            conversation = new Conversation
            {
                CreatedAt = DateTime.UtcNow,
                Messages = new List<Message>()
            };
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync(); // Lưu trước để có Id
        }

        // Thêm tin nhắn của người dùng
        _context.Messages.Add(new Message
        {
            ConversationId = conversation.Id, // Sử dụng Id thay vì object
            Content = userMessage,
            Sender = "User",
            CreatedAt = DateTime.UtcNow
        });

        // Thêm phản hồi của bot
        _context.Messages.Add(new Message
        {
            ConversationId = conversation.Id, // Sử dụng Id thay vì object
            Content = botResponse,
            Sender = "Bot",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return conversation.Id;
    }
    catch (Exception ex)
    {
        // Log lỗi chi tiết
        Console.WriteLine($"Error saving conversation: {ex.Message}");
        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
        throw; // Ném lại ngoại lệ để xử lý ở mức cao hơn
    }
}
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public int? ConversationId { get; set; }
        public bool SaveConversation { get; set; } = true;
    }
}