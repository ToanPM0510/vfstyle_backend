namespace vfstyle_backend.Models.Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } // Casual, Formal, Sport, Fashion, etc.
        public string Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
