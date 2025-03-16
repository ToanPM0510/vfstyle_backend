using System.ComponentModel.DataAnnotations;

namespace vfstyle_backend.DTOs
{
public class ForgotDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
    }
}