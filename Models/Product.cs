
// Models/Product.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vfstyle_backend.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SKU { get; set; } // SKU để liên kết với thư viện model thử kính
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public string Description { get; set; } // Thông tin chi tiết về sản phẩm (tùy chọn)
        
        public int? CategoryId { get; set; }
        
        [StringLength(500)]
        public string ImageUrl { get; set; } // URL hình ảnh sản phẩm (tùy chọn)
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? DeletedAt { get; set; }
        
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }
}