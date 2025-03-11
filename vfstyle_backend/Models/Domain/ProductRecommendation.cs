namespace vfstyle_backend.Models.Domain
{
    public class ProductRecommendation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public int Score { get; set; } // Điểm số phù hợp từ AI
        public string RecommendationReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual Product Product { get; set; }
    }
}
