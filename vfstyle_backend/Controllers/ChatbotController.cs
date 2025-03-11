using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using vfstyle_backend.Data;
using vfstyle_backend.Models.DTOs;
using vfstyle_backend.Services.AI;

namespace vfstyle_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly ApplicationDbContext _context;

        public ChatbotController(IChatbotService chatbotService, ApplicationDbContext context) // Thêm tham số
        {
            _chatbotService = chatbotService;
            _context = context; // Thêm dòng này
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartConversation()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var conversation = await _chatbotService.CreateConversationAsync(userId);

            return Ok(new { conversationId = conversation.Id });
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto messageDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Lưu tin nhắn của người dùng
            await _chatbotService.SaveConversationMessageAsync(
                messageDto.ConversationId,
                messageDto.Message,
                true
            );

            // Lấy gợi ý sản phẩm
            var recommendations = await _chatbotService.GetRecommendationsAsync(messageDto.Message, userId);

            // Tạo phản hồi
            var response = await _chatbotService.GenerateResponseAsync(messageDto.Message, recommendations);

            // Lưu phản hồi của bot
            await _chatbotService.SaveConversationMessageAsync(
                messageDto.ConversationId,
                response,
                false
            );

            return Ok(new
            {
                message = response,
                recommendations = recommendations.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sku = p.Sku,
                    price = p.Price,
                    imageUrl = p.ImageUrl
                }).ToList()
            });
        }

        [HttpGet("history/{conversationId}")]
        public async Task<IActionResult> GetConversationHistory(int conversationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Lấy lịch sử cuộc trò chuyện từ database
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id = m.Id,
                    content = m.Content,
                    isFromUser = m.IsFromUser,
                    sentAt = m.SentAt
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}
