using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using vfstyle_backend.Data;
using vfstyle_backend.DTOs;
using vfstyle_backend.Helpers;
using vfstyle_backend.Models;
using vfstyle_backend.Services;

namespace vfstyle_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(AuthService authService, ApplicationDbContext context, 
            EmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == loginDTO.Username);
            
            if (account == null)
            {
                return BadRequest("Tài khoản không tồn tại.");
            }
            else if (account.DeletedAt != null)
            {
                return BadRequest("Tài khoản đã bị cấm vĩnh viễn.");
            }
            else if (account.Status == "Inactive")
            {
                return BadRequest("Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ admin để tái kích hoạt.");
            }
            
            if (PasswordHelper.VerifyPassword(loginDTO.Password, account.PasswordHash))
                {
                    var token = await _authService.GenerateJwtToken(account, _context);
                    return Ok(new { token, account });
                }
            
            return Unauthorized("Thông tin xác thực không hợp lệ.");
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (await _context.Accounts.AnyAsync(a => a.Username == registerDTO.Username || a.Email == registerDTO.Email))
            {
                return BadRequest("Tên đăng nhập hoặc email đã tồn tại.");
            }

            var account = new Account
            {
                Username = registerDTO.Username,
                Email = registerDTO.Email,
                PasswordHash = PasswordHelper.ToHashPassword(registerDTO.Password),
                EmailVerified = false
            };

            string verificationCode = _authService.GenerateCode();

            _httpContextAccessor.HttpContext.Session.SetString("VerificationCode", verificationCode);
            _httpContextAccessor.HttpContext.Session.SetString("Account", JsonSerializer.Serialize(account));

            try
            {
                string emailBody = $@"
                    <p>Cảm ơn bạn vì đã đăng ký mở tài khoản. Mã xác thực của bạn là:</p>
                    <h2>{verificationCode}</h2>
                ";

                await _emailService.SendEmailAsync(registerDTO.Email, $"Mã xác thực của bạn là: {verificationCode}", emailBody);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Không thể gửi email xác thực.");
            }

            return Ok(new { message = "Đăng ký thành công, email xác thực đã được gửi. Vui lòng kiểm tra hộp thư của bạn." });
        }
public class VerifyCodeRequest
{
    [Required(ErrorMessage = "Mã xác thực là bắt buộc")]
    public string VerifyCode { get; set; }
}

[HttpPost("verify")]
public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    var storedVerificationCode = _httpContextAccessor.HttpContext.Session.GetString("VerificationCode");
    var storedAccount = _httpContextAccessor.HttpContext.Session.GetString("Account");
    
    if (string.IsNullOrEmpty(storedVerificationCode) || string.IsNullOrEmpty(storedAccount))
    {
        return BadRequest("Phiên xác thực đã hết hạn. Vui lòng đăng ký lại.");
    }
    
    var account = JsonSerializer.Deserialize<Account>(storedAccount);
    
    if (request.VerifyCode != storedVerificationCode)  // Sửa từ verifyCode thành VerifyCode
    {
        return BadRequest("Mã xác minh không hợp lệ.");
    }

    account.EmailVerified = true;
    _context.Accounts.Add(account);
    await _context.SaveChangesAsync();

    var token = await _authService.GenerateJwtToken(account, _context);

    _httpContextAccessor.HttpContext.Session.Clear();

    return Ok(new { token, account });
}

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotDTO forgotDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == forgotDTO.Email);
            
            if (account == null)
            {
                return BadRequest("Email không tồn tại.");
            }
            else if (account.DeletedAt != null)
            {
                return BadRequest("Email đã bị cấm vĩnh viễn.");
            }
            else if (account.Status == "Inactive")
            {
                return BadRequest("Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ admin để tái kích hoạt.");
            }
            
            string newPassword = PasswordHelper.RandomPassword();
            account.PasswordHash = PasswordHelper.ToHashPassword(newPassword);
            account.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            try
            {
                string emailBody = $@"
                    <p>Mật khẩu mới của bạn là:</p>
                    <h2>{newPassword}</h2>
                ";
                await _emailService.SendEmailAsync(forgotDTO.Email, "Mật khẩu mới của bạn", emailBody);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Không thể gửi email chứa mật khẩu mới.");
            }
            
            return Ok(new { message = "Mật khẩu mới đã được gửi đến email của bạn." });
        }

        [HttpPost("login/google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                var user = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == payload.Email);
                
                if (user == null)
                {
                    return Unauthorized("Đăng nhập với Google không thành công. Tài khoản Email không tồn tại.");
                }
                else if (user.DeletedAt != null)
                {
                    return Unauthorized("Đăng nhập với Google không thành công. Tài khoản đã bị cấm vĩnh viễn.");
                }
                else if (user.Status == "Inactive")
                {
                    return Unauthorized("Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ admin để tái kích hoạt.");
                }
                
                var token = await _authService.GenerateJwtToken(user, _context);

                return Ok(new { token, user });
            }
            catch (Exception ex)
            {
                return Unauthorized("Đăng nhập với Google không thành công: " + ex.Message);
            }
        }

        [HttpPost("register/google")]
        public async Task<IActionResult> GoogleRegister([FromBody] GoogleRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);

                var user = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == payload.Email);

                if (user != null)
                {
                    return BadRequest("Đăng ký với Google không thành công. Email đã đăng ký.");
                }

                user = new Account
                {
                    Username = payload.Email.Split('@')[0],
                    Email = payload.Email,
                    PasswordHash = PasswordHelper.ToHashPassword(PasswordHelper.RandomPassword()),
                    EmailVerified = true // Đã xác minh qua Google
                };
                
                _context.Accounts.Add(user);
                await _context.SaveChangesAsync();
                
                var token = await _authService.GenerateJwtToken(user, _context);

                return Ok(new { token, user });
            }
            catch (Exception ex)
            {
                return Unauthorized("Đăng ký với Google không thành công: " + ex.Message);
            }
        }
    }
}