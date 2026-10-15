using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using System.Security.Claims;
using static RaceDayAPI.Dtos.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDayAPI.Controllers
{
    [ApiController, Route("api/users"), Authorize]
    public class UsersController(AppDbContext db) : ControllerBase
    {
        int CallerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>View the logged-in user's own profile.</summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMe()
        {
            var me = await db.Users.Where(u => u.UserId == CallerId)
                .Select(u => new {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    Role = u.Role.RoleName,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();
            return me == null ? NotFound() : Ok(me);
        }

        /// <summary>Update the logged-in user's own name and phone number.</summary>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateMe(ProfileDto dto)
        {
            var user = await db.Users.FindAsync(CallerId);
            if (user == null) return NotFound();
            if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest("Full name is required.");
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
