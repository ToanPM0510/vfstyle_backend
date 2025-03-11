namespace vfstyle_backend.Models.DTOs
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Style { get; set; }
        public string Material { get; set; }
        public string FaceShapeRecommendation { get; set; }
        public int? CategoryId { get; set; }
        public string Keywords { get; set; }
    }
}
