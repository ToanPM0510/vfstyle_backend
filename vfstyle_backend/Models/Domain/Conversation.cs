namespace vfstyle_backend.Models.Domain
{
    public class Conversation
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}
