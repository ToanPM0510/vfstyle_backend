// DTOs/ProductDTO.cs
namespace vfstyle_backend.DTOs
{
public class ProductDTO
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateProductDTO
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateProductDTO
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int? CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
    }
}

