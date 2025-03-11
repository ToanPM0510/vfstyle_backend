namespace vfstyle_backend.Models.Domain
{
    public class Product
    {
        public string Id { get; set; } // e.g., "model2"
        public string Sku { get; set; } // e.g., "rayban_round_cuivre_pinkBrownDegrade"
        public string Name { get; set; } // e.g., "Rayban Round"
        public string Price { get; set; } // e.g., "₫300.000"
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Thêm các thuộc tính để phân loại kính
        public int? CategoryId { get; set; }
        public string Style { get; set; } // Round, Square, Aviator, etc.
        public string Material { get; set; } // Metal, Plastic, etc.
        public string FaceShapeRecommendation { get; set; } // Round, Oval, Square, etc.
        public string Keywords { get; set; } // Từ khóa để matching, lưu dạng JSON

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<ProductRecommendation> ProductRecommendations { get; set; }
    }
}
