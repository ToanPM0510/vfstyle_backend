using System.ComponentModel.DataAnnotations;

namespace vfstyle_backend.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
    
    public class UserRole
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int RoleId { get; set; }
        
        public virtual Account Account { get; set; }
        public virtual Role Role { get; set; }
    }
}
