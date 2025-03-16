using System.ComponentModel.DataAnnotations;

namespace vfstyle_backend.DTOs
{
public class LoginDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }
    }
}