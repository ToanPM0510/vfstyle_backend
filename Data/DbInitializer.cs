using vfstyle_backend.Models;
using vfstyle_backend.Helpers;

namespace vfstyle_backend.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            
            // Kiểm tra xem đã có roles chưa
            if (!context.Roles.Any())
            {
                var roles = new Role[]
                {
                    new Role { Name = "Admin", Description = "Quản trị viên hệ thống" },
                    new Role { Name = "User", Description = "Người dùng thông thường" }
                };
                
                foreach (var role in roles)
                {
                    context.Roles.Add(role);
                }
                
                context.SaveChanges();
            }
            
            // Tạo tài khoản admin mặc định nếu chưa có
            if (!context.Accounts.Any(a => a.Username == "admin"))
            {
                var adminAccount = new Account
                {
                    Username = "admin",
                    Email = "admin@vfstyle.com",
                    PasswordHash = PasswordHelper.ToHashPassword("Admin@123"),
                    Status = "Active",
                    EmailVerified = true
                };
                
                context.Accounts.Add(adminAccount);
                context.SaveChanges();
                
                // Gán vai trò Admin cho tài khoản admin
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                
                if (adminRole != null)
                {
                    var userRole = new UserRole
                    {
                        AccountId = adminAccount.Id,
                        RoleId = adminRole.Id
                    };
                    
                    context.UserRoles.Add(userRole);
                    context.SaveChanges();
                }
            }
        }
    }
}