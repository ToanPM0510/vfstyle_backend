namespace vfstyle_backend.Models.Domain
{
    public class CustomerPreference
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Style { get; set; } // e.g., "casual", "formal", "sporty"
        public string FaceShape { get; set; } // e.g., "round", "oval", "square"
        public string ColorPreference { get; set; }
        public string AdditionalRequirements { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
    }
}
