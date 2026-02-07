using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.Auth;
using TaskApi.Data;
using TaskApi.DTO;
using TaskApi.Entities;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Name,
                    role = user.Role
                }
            });
        }
        
        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUsers()
        {
            var list = await _db.Users
                .Select(u => new { u.Id, u.Email, u.Role })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost("users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("User with this email already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "member" : dto.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.Email, user.Role });
        }

        [HttpPut("users/{id}/role")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = dto.Role;
            await _db.SaveChangesAsync();

            return Ok(new { user.Id, user.Email, user.Role });
        }

        [HttpDelete("users/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        
        public class CreateUserDto
        {
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string? Role { get; set; }
        }

        public class UpdateRoleDto
        {
            public string Role { get; set; } = null!;
        }
    }
}
