using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using vfstyle_backend.Models.Domain;

namespace vfstyle_backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CustomerPreference> CustomerPreferences { get; set; }
        public DbSet<ProductRecommendation> ProductRecommendations { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<CustomerPreference>()
                .HasOne(p => p.User)
                .WithMany(u => u.CustomerPreferences)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductRecommendation>()
                .HasOne(r => r.User)
                .WithMany(u => u.ProductRecommendations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductRecommendation>()
                .HasOne(r => r.Product)
                .WithMany(p => p.ProductRecommendations)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            builder.Entity<Conversation>()
                .HasOne(c => c.User)
                .WithMany(u => u.Conversations)
                .HasForeignKey(c => c.UserId);

            builder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);
        }
    }
}
