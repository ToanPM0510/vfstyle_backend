// Models/CustomerPreference.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vfstyle_backend.Models
{
    public class CustomerPreference
    {
        [Key]
        public int Id { get; set; }
        
        public int? AccountId { get; set; }
        
        public string SessionId { get; set; } // Cho người dùng chưa đăng nhập
        
        public string SearchTerm { get; set; }
        
        public string Category { get; set; }
        
        public decimal? PriceMin { get; set; }
        
        public decimal? PriceMax { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}