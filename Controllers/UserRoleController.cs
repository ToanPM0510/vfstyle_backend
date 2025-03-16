using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using vfstyle_backend.Data;
using vfstyle_backend.Models;

namespace vfstyle_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserRoleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        public UserRoleController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet("user/{accountId}")]
        public async Task<ActionResult<IEnumerable<Role>>> GetUserRoles(int accountId)
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.AccountId == accountId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role)
                .ToListAsync();
                
            return roles;
        }
        
        [HttpPost]
        public async Task<ActionResult<UserRole>> AssignRole(UserRole userRole)
        {
            // Kiểm tra xem user và role có tồn tại không
            var account = await _context.Accounts.FindAsync(userRole.AccountId);
            var role = await _context.Roles.FindAsync(userRole.RoleId);
            
            if (account == null || role == null)
            {
                return BadRequest("Tài khoản hoặc vai trò không tồn tại.");
            }
            
            // Kiểm tra xem đã có assignment này chưa
            var existingAssignment = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.AccountId == userRole.AccountId && ur.RoleId == userRole.RoleId);
                
            if (existingAssignment != null)
            {
                return BadRequest("Người dùng đã có vai trò này.");
            }
            
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("GetUserRoles", new { accountId = userRole.AccountId }, userRole);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveRole(int id)
        {
            var userRole = await _context.UserRoles.FindAsync(id);
            
            if (userRole == null)
            {
                return NotFound();
            }
            
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}