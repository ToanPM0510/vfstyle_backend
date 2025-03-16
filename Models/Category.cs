// Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace vfstyle_backend.Models
{
public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public virtual ICollection<Product> Products { get; set; }
    }
}
