using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = "Admin")] // Chỉ admin mới có quyền quản lý tài khoản
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        
        public AccountsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            if (_context.Accounts == null)
            {
                return NotFound();
            }
            return await _context.Accounts.Where(a => a.DeletedAt == null).ToListAsync();
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(int id)
        {
            if (_context.Accounts == null)
            {
                return NotFound();
            }
            var account = await _context.Accounts.FindAsync(id);

            if (account == null || account.DeletedAt != null)
            {
                return NotFound();
            }

            return account;
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(RegisterDTO accountDTO)
        {
            if (_context.Accounts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Accounts' is null.");
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var account = await _context.Accounts.FirstOrDefaultAsync(a => 
                a.Email == accountDTO.Email || a.Username == accountDTO.Username);
                
            if (account != null)
            {
                return BadRequest("Tên đăng nhập hoặc email đã tồn tại. Tạo tài khoản mới thất bại.");
            }
            
            var newPassword = PasswordHelper.RandomPassword();
            
            account = new Account
            {
                Username = accountDTO.Username,
                Email = accountDTO.Email,
                PasswordHash = PasswordHelper.ToHashPassword(newPassword),
                EmailVerified = true // Admin tạo tài khoản nên mặc định đã xác minh
            };
            
            try
            {
                string emailBody = $@"
                    <h1>Chào mừng bạn đến với VF Style</h1>
                    <p>Tài khoản của bạn đã được tạo thành công.</p>
                    <p>Mật khẩu mới của bạn là:</p>
                    <h2>{newPassword}</h2>
                ";
                await _emailService.SendEmailAsync(account.Email, "Tài khoản được tạo bởi Admin", emailBody);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Không thể gửi email.");
            }
            
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("GetAccount", new { id = account.Id }, account);
        }

        // PUT: api/Accounts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(int id, Account account)
        {
            if (id != account.Id)
            {
                return BadRequest();
            }

            account.UpdatedAt = DateTime.UtcNow;
            _context.Entry(account).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            if (_context.Accounts == null)
            {
                return NotFound();
            }
            
            var account = await _context.Accounts.FindAsync(id);
            
            if (account == null)
            {
                return NotFound();
            }

            // Soft delete
            account.DeletedAt = DateTime.UtcNow;
            account.Status = "Inactive";
            
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExists(int id)
        {
            return (_context.Accounts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}