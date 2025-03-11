using vfstyle_backend.Models.Domain;

namespace vfstyle_backend.Services.AI
{
    public interface IChatbotService
    {
        Task<List<Product>> GetRecommendationsAsync(string userMessage, string userId);
        Task<string> GenerateResponseAsync(string userMessage, List<Product> recommendations);
        Task SaveConversationMessageAsync(int conversationId, string message, bool isFromUser);
        Task<Conversation> CreateConversationAsync(string userId);
    }
}
