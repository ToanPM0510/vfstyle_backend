using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using vfstyle_backend.Data;
using vfstyle_backend.Models.Domain;

namespace vfstyle_backend
{
    public static class SeedData
    {
        public static async Task Initialize(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Seed Admin User
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailVerified = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Casual", Description = "Kính mắt phong cách thường ngày" },
                    new Category { Name = "Formal", Description = "Kính mắt phong cách công sở" },
                    new Category { Name = "Sport", Description = "Kính mắt thể thao" },
                    new Category { Name = "Fashion", Description = "Kính mắt thời trang" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!context.Products.Any())
            {
                var casualCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Casual");
                var formalCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Formal");
                var sportCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sport");
                var fashionCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Fashion");

                var products = new List<Product>
                {
                    new Product
                    {
                        Id = "model1",
                        Sku = "rayban_wayfarer_black",
                        Name = "Rayban Wayfarer",
                        Price = "₫250.000",
                        Description = "Kính mắt Rayban Wayfarer màu đen cổ điển",
                        ImageUrl = "https://example.com/images/rayban_wayfarer.jpg",
                        Style = "Square",
                        Material = "Plastic",
                        FaceShapeRecommendation = "Round,Oval",
                        CategoryId = casualCategory.Id,
                        Keywords = "rayban,wayfarer,black,casual,classic",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = "model2",
                        Sku = "rayban_round_cuivre_pinkBrownDegrade",
                        Name = "Rayban Round",
                        Price = "₫300.000",
                        Description = "Kính mắt Rayban Round màu đồng với tròng kính hồng nâu",
                        ImageUrl = "https://example.com/images/rayban_round.jpg",
                        Style = "Round",
                        Material = "Metal",
                        FaceShapeRecommendation = "Square,Heart",
                        CategoryId = fashionCategory.Id,
                        Keywords = "rayban,round,pink,brown,fashion,metal",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = "model3",
                        Sku = "oakley_holbrook_matte_black",
                        Name = "Oakley Holbrook",
                        Price = "₫350.000",
                        Description = "Kính mắt Oakley Holbrook màu đen mờ cho hoạt động thể thao",
                        ImageUrl = "https://example.com/images/oakley_holbrook.jpg",
                        Style = "Square",
                        Material = "Plastic",
                        FaceShapeRecommendation = "Round,Oval",
                        CategoryId = sportCategory.Id,
                        Keywords = "oakley,holbrook,black,sport,active",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = "model4",
                        Sku = "persol_649_havana",
                        Name = "Persol 649",
                        Price = "₫400.000",
                        Description = "Kính mắt Persol 649 màu Havana sang trọng",
                        ImageUrl = "https://example.com/images/persol_649.jpg",
                        Style = "Aviator",
                        Material = "Acetate",
                        FaceShapeRecommendation = "Square,Oval",
                        CategoryId = formalCategory.Id,
                        Keywords = "persol,649,havana,formal,luxury",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
