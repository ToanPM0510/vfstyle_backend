using Microsoft.EntityFrameworkCore;
using vfstyle_backend.Data;
using vfstyle_backend.Models.Domain;

namespace vfstyle_backend.Services.AI
{
    public class ChatbotService : IChatbotService
    {
        private readonly ApplicationDbContext _context;

        public ChatbotService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetRecommendationsAsync(string userMessage, string userId)
        {
            // Phân tích tin nhắn người dùng để trích xuất thông tin
            var keywords = ExtractKeywords(userMessage);
            var style = DetectStyle(userMessage);
            var faceShape = DetectFaceShape(userMessage);
            var colorPreference = DetectColorPreference(userMessage);

            // Lưu preference của người dùng
            await SaveUserPreference(userId, style, faceShape, colorPreference, userMessage);

            // Tìm kiếm sản phẩm phù hợp
            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(style))
            {
                query = query.Where(p => p.Style.Contains(style));
            }

            if (!string.IsNullOrEmpty(faceShape))
            {
                query = query.Where(p => p.FaceShapeRecommendation.Contains(faceShape));
            }

            if (!string.IsNullOrEmpty(colorPreference))
            {
                query = query.Where(p => p.Sku.Contains(colorPreference.ToLower()) ||
                                        p.Description.Contains(colorPreference));
            }

            // Tìm kiếm theo từ khóa
            if (keywords.Any())
            {
                query = query.Where(p =>
                    keywords.Any(k => p.Name.Contains(k) ||
                                     p.Description.Contains(k) ||
                                     p.Keywords.Contains(k)));
            }

            return await query.Take(5).ToListAsync();
        }

        public async Task<string> GenerateResponseAsync(string userMessage, List<Product> recommendations)
        {
            if (!recommendations.Any())
            {
                return "Xin lỗi, tôi không tìm thấy kính phù hợp với yêu cầu của bạn. Bạn có thể mô tả chi tiết hơn về phong cách hoặc nhu cầu của mình không?";
            }

            var response = "Dựa trên thông tin bạn cung cấp, tôi gợi ý những mẫu kính sau:\n\n";

            foreach (var product in recommendations)
            {
                response += $"- {product.Name}: {product.Price}\n";
                response += $"  SKU: {product.Sku}\n";
                if (!string.IsNullOrEmpty(product.Description))
                {
                    response += $"  Mô tả: {product.Description}\n";
                }
                response += "\n";
            }

            response += "Bạn có muốn biết thêm thông tin về mẫu kính nào không?";

            return response;
        }

        public async Task SaveConversationMessageAsync(int conversationId, string message, bool isFromUser)
        {
            var newMessage = new Message
            {
                ConversationId = conversationId,
                Content = message,
                IsFromUser = isFromUser,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();
        }

        public async Task<Conversation> CreateConversationAsync(string userId)
        {
            var conversation = new Conversation
            {
                UserId = userId,
                StartedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }

        private async Task SaveUserPreference(string userId, string style, string faceShape, string colorPreference, string additionalRequirements)
        {
            var preference = new CustomerPreference
            {
                UserId = userId,
                Style = style,
                FaceShape = faceShape,
                ColorPreference = colorPreference,
                AdditionalRequirements = additionalRequirements,
                CreatedAt = DateTime.UtcNow
            };

            _context.CustomerPreferences.Add(preference);
            await _context.SaveChangesAsync();
        }

        private List<string> ExtractKeywords(string message)
        {
            // Đơn giản hóa: tách các từ và lọc các từ có ý nghĩa
            var words = message.ToLower().Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            var keywords = new List<string>();
            var keywordDictionary = new Dictionary<string, string[]>
            {
                { "thời trang", new[] { "thời trang", "fashion", "phong cách", "style" } },
                { "thể thao", new[] { "thể thao", "sport", "chạy", "đạp xe", "bơi" } },
                { "công sở", new[] { "công sở", "văn phòng", "formal", "business", "làm việc" } },
                { "casual", new[] { "casual", "hàng ngày", "thường ngày", "đời thường" } }
            };

            foreach (var word in words)
            {
                foreach (var category in keywordDictionary)
                {
                    if (category.Value.Contains(word))
                    {
                        keywords.Add(category.Key);
                        break;
                    }
                }
            }

            return keywords.Distinct().ToList();
        }

        private string DetectStyle(string message)
        {
            message = message.ToLower();

            if (message.Contains("tròn") || message.Contains("round"))
                return "Round";

            if (message.Contains("vuông") || message.Contains("square"))
                return "Square";

            if (message.Contains("phi công") || message.Contains("aviator"))
                return "Aviator";

            if (message.Contains("cat eye") || message.Contains("mắt mèo"))
                return "Cat Eye";

            return null;
        }

        private string DetectFaceShape(string message)
        {
            message = message.ToLower();

            if (message.Contains("mặt tròn") || message.Contains("round face"))
                return "Round";

            if (message.Contains("mặt vuông") || message.Contains("square face"))
                return "Square";

            if (message.Contains("mặt oval") || message.Contains("oval face"))
                return "Oval";

            if (message.Contains("mặt trái xoan") || message.Contains("heart face"))
                return "Heart";

            return null;
        }

        private string DetectColorPreference(string message)
        {
            message = message.ToLower();

            if (message.Contains("đen") || message.Contains("black"))
                return "Black";

            if (message.Contains("nâu") || message.Contains("brown"))
                return "Brown";

            if (message.Contains("vàng") || message.Contains("gold"))
                return "Gold";

            if (message.Contains("bạc") || message.Contains("silver"))
                return "Silver";

            if (message.Contains("xanh") || message.Contains("blue"))
                return "Blue";

            if (message.Contains("hồng") || message.Contains("pink"))
                return "Pink";

            return null;
        }
    }
}
