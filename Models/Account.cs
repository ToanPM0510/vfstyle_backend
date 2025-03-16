using System;
using System.ComponentModel.DataAnnotations;

namespace vfstyle_backend.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public string Status { get; set; } = "Active"; // Active, Inactive
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? DeletedAt { get; set; }
        
        public bool EmailVerified { get; set; } = false;
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}