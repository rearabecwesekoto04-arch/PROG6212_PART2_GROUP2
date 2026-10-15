using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace RaceDayAPI.Controllers
{
    [ApiController, Route("api/auth")]
    public class AuthController(AppDbContext db, IConfiguration cfg) : ControllerBase
    {

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await db.Users.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already registered.");
            var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null) return BadRequest("Role must be Organiser or Participant.");

            db.Users.Add(new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.Phone,
                RoleId = role.RoleId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            });
            await db.SaveChangesAsync();
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await db.Users.Include(u => u.Role)
                         .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.RoleName) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
            var token = new JwtSecurityToken(cfg["Jwt:Issuer"], cfg["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

    public record RegisterDto(string FullName, string Email, string Password, string Role, string? Phone);
    public record LoginDto(string Email, string Password);
}
