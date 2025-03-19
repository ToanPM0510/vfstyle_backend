using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vfstyle_backend.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        
        public int? AccountId { get; set; }
        
        public string SessionId { get; set; } // Cho người dùng chưa đăng nhập
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
        
        public virtual ICollection<Message> Messages { get; set; }
    }
}