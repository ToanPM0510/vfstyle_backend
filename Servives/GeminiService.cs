// Services/GeminiService.cs
using GenerativeAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace vfstyle_backend.Services
{
    public interface ILLMService
    {
        Task<string> GetChatCompletionAsync(List<(string role, string content)> messages);
    }

    public class GeminiService : ILLMService
    {
        private readonly string _apiKey;
        private readonly GenerativeModel _model;

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _model = new GenerativeModel("gemini-pro", _apiKey);
        }

        public async Task<string> GetChatCompletionAsync(List<(string role, string content)> messages)
        {
            try
            {
                // Xây dựng prompt từ các tin nhắn
                string prompt = BuildPromptFromMessages(messages);

                // Gọi API để lấy phản hồi
                var response = await _model.GenerateContentAsync(prompt);

                return (string)response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Gemini API: {ex.Message}");
                return "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng thử lại sau.";
            }
        }

        private string BuildPromptFromMessages(List<(string role, string content)> messages)
        {
            var systemMessages = messages.Where(m => m.role.Equals("system", StringComparison.OrdinalIgnoreCase)).ToList();
            var userMessages = messages.Where(m => m.role.Equals("user", StringComparison.OrdinalIgnoreCase)).ToList();
            var assistantMessages = messages.Where(m => m.role.Equals("assistant", StringComparison.OrdinalIgnoreCase)).ToList();

            string prompt = "";

            // Thêm system message nếu có
            if (systemMessages.Any())
            {
                prompt += "Instructions:\n";
                foreach (var (_, content) in systemMessages)
                {
                    prompt += content + "\n";
                }
                prompt += "\n";
            }

            // Thêm lịch sử hội thoại
            for (int i = 0; i < Math.Max(userMessages.Count, assistantMessages.Count); i++)
            {
                if (i < userMessages.Count)
                {
                    prompt += "User: " + userMessages[i].content + "\n";
                }

                if (i < assistantMessages.Count)
                {
                    prompt += "Assistant: " + assistantMessages[i].content + "\n";
                }
            }

            // Thêm tin nhắn cuối cùng của người dùng nếu có
            var lastUserMessage = userMessages.LastOrDefault();
            if (lastUserMessage != default && !prompt.EndsWith("User: " + lastUserMessage.content + "\n"))
            {
                prompt += "User: " + lastUserMessage.content + "\n";
            }

            prompt += "Assistant: ";

            return prompt;
        }
    }
}