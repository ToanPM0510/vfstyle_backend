namespace vfstyle_backend.Models.Domain
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public bool IsFromUser { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public virtual Conversation Conversation { get; set; }
    }
}
