// Services/ChatbotService.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vfstyle_backend.Data;
using vfstyle_backend.DTOs;
using vfstyle_backend.Models;

namespace vfstyle_backend.Services
{
    public interface IChatbotService
    {
        Task<ConversationDTO> StartConversation(int? accountId, string sessionId);
        Task<MessageDTO> ProcessMessage(int conversationId, string message, int? accountId, string sessionId);
        Task<ConversationDTO> GetConversation(int conversationId);
        Task<List<ConversationDTO>> GetConversations(int? accountId, string sessionId);
    }

    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILLMService _llmService;
        
        public ChatbotService(ApplicationDbContext context, ILLMService llmService)
        {
            _context = context;
            _llmService = llmService;
        }
        
        public async Task<ConversationDTO> StartConversation(int? accountId, string sessionId)
        {
            var conversation = new Conversation
            {
                AccountId = accountId,
                SessionId = sessionId,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Content = "Xin chào! Tôi là trợ lý ảo của VF Style. Tôi có thể giúp bạn tìm kiếm kính mắt phù hợp. Bạn đang tìm kiếm loại kính như thế nào?",
                        Sender = "Bot",
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };
            
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            
            return new ConversationDTO
            {
                Id = conversation.Id,
                Messages = conversation.Messages.Select(m => new MessageDTO
                {
                    Content = m.Content,
                    Sender = m.Sender,
                    CreatedAt = m.CreatedAt
                }).ToList()
            };
        }
        
        public async Task<MessageDTO> ProcessMessage(int conversationId, string message, int? accountId, string sessionId)
        {
            // Lưu tin nhắn của người dùng
            var userMessage = new Message
            {
                ConversationId = conversationId,
                Content = message,
                Sender = "User",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Messages.Add(userMessage);
            await _context.SaveChangesAsync();
            
            // Lấy lịch sử cuộc trò chuyện
            var conversation = await _context.Conversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == conversationId);
                
            if (conversation == null)
            {
                throw new Exception("Conversation not found");
            }
            
            // Chuẩn bị tin nhắn cho Gemini API
            var chatMessages = new List<(string role, string content)>();
            
            // Thêm system message để định hướng chatbot
            chatMessages.Add(("system", 
                "Bạn là trợ lý ảo của VF Style, một cửa hàng bán kính mắt. " +
                "Nhiệm vụ của bạn là giúp khách hàng tìm kiếm kính mắt phù hợp. " +
                "Hãy trả lời thân thiện, ngắn gọn và hữu ích bằng tiếng Việt. " +
                "Nếu khách hàng hỏi về sản phẩm cụ thể, hãy đề xuất họ xem các sản phẩm trên website. " +
                "Nếu khách hàng hỏi về giá cả, hãy cung cấp thông tin về khoảng giá của các sản phẩm. " +
                "Nếu khách hàng hỏi về cách chọn kính phù hợp, hãy đưa ra lời khuyên hữu ích."));
            
            // Thêm lịch sử tin nhắn (tối đa 10 tin nhắn gần nhất để tránh vượt quá token limit)
            foreach (var msg in conversation.Messages.OrderByDescending(m => m.CreatedAt).Take(10).OrderBy(m => m.CreatedAt))
            {
                var role = msg.Sender == "User" ? "user" : "assistant";
                chatMessages.Add((role, msg.Content));
            }
            
            // Gọi Gemini API để lấy phản hồi
            string botResponse = await _llmService.GetChatCompletionAsync(chatMessages);
            
            // Phân tích tin nhắn để trích xuất sở thích
            var preferences = ExtractPreferences(message);
            
            // Lưu hoặc cập nhật sở thích của khách hàng
            await SaveCustomerPreferences(accountId, sessionId, preferences);
            
            // Nếu có đủ thông tin về sở thích, tìm kiếm sản phẩm phù hợp
            if (preferences.Count > 0)
            {
                var recommendedProducts = await FindRecommendedProducts(preferences);
                
                if (recommendedProducts.Count > 0)
                {
                    botResponse += "\n\nDựa trên yêu cầu của bạn, tôi đề xuất các sản phẩm sau:\n";
                    
                    foreach (var product in recommendedProducts.Take(3))
                    {
                        botResponse += $"\n- {product.Name}: {product.Price:N0}đ - {product.Description}";
                    }
                    
                    if (recommendedProducts.Count > 3)
                    {
                        botResponse += $"\n\nVà {recommendedProducts.Count - 3} sản phẩm khác. Bạn có muốn xem thêm không?";
                    }
                }
            }
            
            // Lưu phản hồi của bot
            var botMessage = new Message
            {
                ConversationId = conversationId,
                Content = botResponse,
                Sender = "Bot",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Messages.Add(botMessage);
            await _context.SaveChangesAsync();
            
            return new MessageDTO
            {
                Content = botResponse,
                Sender = "Bot",
                CreatedAt = botMessage.CreatedAt
            };
        }
        
        public async Task<ConversationDTO> GetConversation(int conversationId)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
                
            if (conversation == null)
            {
                return null;
            }
            
            return new ConversationDTO
            {
                Id = conversation.Id,
                Messages = conversation.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageDTO
                    {
                        Content = m.Content,
                        Sender = m.Sender,
                        CreatedAt = m.CreatedAt
                    }).ToList()
            };
        }
        
        public async Task<List<ConversationDTO>> GetConversations(int? accountId, string sessionId)
        {
            var query = _context.Conversations.AsQueryable();
            
            if (accountId.HasValue)
            {
                query = query.Where(c => c.AccountId == accountId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(c => c.SessionId == sessionId);
            }
            else
            {
                return new List<ConversationDTO>();
            }
            
            var conversations = await query
                .Include(c => c.Messages)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();
                
            return conversations.Select(c => new ConversationDTO
            {
                Id = c.Id,
                Messages = c.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new MessageDTO
                    {
                        Content = m.Content,
                        Sender = m.Sender,
                        CreatedAt = m.CreatedAt
                    }).ToList()
            }).ToList();
        }
        
        private Dictionary<string, string> ExtractPreferences(string message)
        {
            var preferences = new Dictionary<string, string>();
            
            // Trích xuất từ khóa tìm kiếm
            string[] keywords = { "kính", "mắt kính", "gọng kính", "rayban", "oakley", "gucci" };
            foreach (var keyword in keywords)
            {
                if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    preferences["SearchTerm"] = keyword;
                    break;
                }
            }
            
            // Trích xuất danh mục
            string[] categories = { "kính mát", "kính cận", "kính thời trang" };
            foreach (var category in categories)
            {
                if (message.Contains(category, StringComparison.OrdinalIgnoreCase))
                {
                    preferences["Category"] = category;
                    break;
                }
            }
            
            // Trích xuất khoảng giá
            var priceMatch = Regex.Match(message, @"(dưới|từ|trên)\s*(\d+)(\s*nghìn|\s*ngàn|\s*k|\s*triệu|\s*tr)?", RegexOptions.IgnoreCase);
            if (priceMatch.Success)
            {
                string priceType = priceMatch.Groups[1].Value.ToLower();
                string priceValue = priceMatch.Groups[2].Value;
                string priceUnit = priceMatch.Groups[3].Value.ToLower();
                
                decimal multiplier = 1;
                if (priceUnit.Contains("nghìn") || priceUnit.Contains("ngàn") || priceUnit.Contains("k"))
                {
                    multiplier = 1000;
                }
                else if (priceUnit.Contains("triệu") || priceUnit.Contains("tr"))
                {
                    multiplier = 1000000;
                }
                
                if (decimal.TryParse(priceValue, out decimal price))
                {
                    price *= multiplier;
                    
                    if (priceType == "dưới")
                    {
                        preferences["PriceMax"] = price.ToString();
                    }
                    else if (priceType == "trên")
                    {
                        preferences["PriceMin"] = price.ToString();
                    }
                    else if (priceType == "từ")
                    {
                        // Nếu có "đến" hoặc "tới" sau "từ"
                        var toMatch = Regex.Match(message, @"từ\s*\d+(\s*nghìn|\s*ngàn|\s*k|\s*triệu|\s*tr)?\s*(đến|tới)\s*(\d+)(\s*nghìn|\s*ngàn|\s*k|\s*triệu|\s*tr)?", RegexOptions.IgnoreCase);
                        if (toMatch.Success)
                        {
                            string maxPriceValue = toMatch.Groups[3].Value;
                            string maxPriceUnit = toMatch.Groups[4].Value.ToLower();
                            
                            decimal maxMultiplier = 1;
                            if (maxPriceUnit.Contains("nghìn") || maxPriceUnit.Contains("ngàn") || maxPriceUnit.Contains("k"))
                            {
                                maxMultiplier = 1000;
                            }
                            else if (maxPriceUnit.Contains("triệu") || maxPriceUnit.Contains("tr"))
                            {
                                maxMultiplier = 1000000;
                            }
                            
                            if (decimal.TryParse(maxPriceValue, out decimal maxPrice))
                            {
                                maxPrice *= maxMultiplier;
                                preferences["PriceMin"] = price.ToString();
                                preferences["PriceMax"] = maxPrice.ToString();
                            }
                        }
                        else
                        {
                            preferences["PriceMin"] = price.ToString();
                        }
                    }
                }
            }
            
            return preferences;
        }
        
        private async Task SaveCustomerPreferences(int? accountId, string sessionId, Dictionary<string, string> preferences)
        {
            if (preferences.Count == 0)
            {
                return;
            }

            var customerPreference = await _context.CustomerPreferences
                .FirstOrDefaultAsync(cp => 
                    (accountId.HasValue && cp.AccountId == accountId) || 
                    (!accountId.HasValue && cp.SessionId == sessionId));

            if (customerPreference == null)
            {
                customerPreference = new CustomerPreference
                {
                    AccountId = accountId,
                    SessionId = sessionId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CustomerPreferences.Add(customerPreference);
            }
            else
            {
                customerPreference.UpdatedAt = DateTime.UtcNow;
            }

            // Cập nhật các thuộc tính từ preferences
            if (preferences.ContainsKey("SearchTerm"))
            {
                customerPreference.SearchTerm = preferences["SearchTerm"];
            }
            if (preferences.ContainsKey("Category"))
            {
                customerPreference.Category = preferences["Category"];
            }
            if (preferences.ContainsKey("PriceMin"))
            {
                if (decimal.TryParse(preferences["PriceMin"], out decimal priceMin))
                {
                    customerPreference.PriceMin = priceMin;
                }
            }
            if (preferences.ContainsKey("PriceMax"))
            {
                if (decimal.TryParse(preferences["PriceMax"], out decimal priceMax))
                {
                    customerPreference.PriceMax = priceMax;
                }
            }

            await _context.SaveChangesAsync();
        }
        
        private async Task<List<Product>> FindRecommendedProducts(Dictionary<string, string> preferences)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.DeletedAt == null && p.IsAvailable);

            // Tìm kiếm dựa trên tên sản phẩm hoặc mô tả
            if (preferences.ContainsKey("SearchTerm"))
            {
                string searchTerm = preferences["SearchTerm"];
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                        p.Description.Contains(searchTerm) || 
                                        p.SKU.Contains(searchTerm));
            }

            // Tìm kiếm theo danh mục
            if (preferences.ContainsKey("Category"))
            {
                string category = preferences["Category"];
                query = query.Where(p => p.Category.Name.Contains(category));
            }

            // Tìm kiếm theo khoảng giá
            if (preferences.ContainsKey("PriceMin") && decimal.TryParse(preferences["PriceMin"], out decimal priceMin))
            {
                query = query.Where(p => p.Price >= priceMin);
            }
            if (preferences.ContainsKey("PriceMax") && decimal.TryParse(preferences["PriceMax"], out decimal priceMax))
            {
                query = query.Where(p => p.Price <= priceMax);
            }

            return await query.ToListAsync();
        }
    }
}