using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using vfstyle_backend.Models.Domain;
using vfstyle_backend.Models.DTOs;

namespace vfstyle_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("Mật khẩu xác nhận không khớp.");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
    }
}
