// Models/Message.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vfstyle_backend.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        
        public int ConversationId { get; set; }
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        public string Sender { get; set; } // "User" hoặc "Bot"
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }
    }
}