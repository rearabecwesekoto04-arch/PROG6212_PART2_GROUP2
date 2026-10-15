using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;
using static RaceDayAPI.Dtos.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RaceDayAPI.Controllers
{
    [ApiController, Route("api/categories"), Authorize]
    public class CategoriesController(AppDbContext db) : ControllerBase
    {
        int CallerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        static readonly string[] ValidTypes = ["Age", "Distance"];

        /// <summary>List categories, optionally filtered by event.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? eventId)
        {
            var q = db.Categories.AsQueryable();
            if (eventId != null) q = q.Where(c => c.EventId == eventId);
            return Ok(await q.Select(c => new {
                c.CategoryId,
                c.EventId,
                c.CategoryName,
                c.CategoryType,
                c.DistanceKm,
                c.MinAge,
                c.MaxAge,
                c.Price,
                c.MaxParticipants
            })
                .ToListAsync());
        }

        /// <summary>Get a single category.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var c = await db.Categories.Where(x => x.CategoryId == id)
                .Select(x => new {
                    x.CategoryId,
                    x.EventId,
                    x.CategoryName,
                    x.CategoryType,
                    x.DistanceKm,
                    x.MinAge,
                    x.MaxAge,
                    x.Price,
                    x.MaxParticipants
                })
                .FirstOrDefaultAsync();
            return c == null ? NotFound() : Ok(c);
        }

        /// <summary>Organiser: add a category to one of their events.</summary>
        [HttpPost, Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(CategoryDto d)
        {
            if (!ValidTypes.Contains(d.CategoryType))
                return BadRequest("CategoryType must be 'Age' or 'Distance'.");
            var ev = await db.Events.FindAsync(d.EventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.OrganiserId != CallerId) return Forbid();

            var cat = new Category
            {
                EventId = d.EventId,
                CategoryName = d.CategoryName,
                CategoryType = d.CategoryType,
                DistanceKm = d.DistanceKm,
                MinAge = d.MinAge,
                MaxAge = d.MaxAge,
                Price = d.Price,
                MaxParticipants = d.MaxParticipants
            };
            db.Categories.Add(cat);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = cat.CategoryId }, new { cat.CategoryId });
        }

        /// <summary>Organiser: update a category on one of their events.</summary>
        [HttpPut("{id}"), Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, CategoryDto d)
        {
            if (!ValidTypes.Contains(d.CategoryType))
                return BadRequest("CategoryType must be 'Age' or 'Distance'.");
            var cat = await db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (cat == null) return NotFound();
            if (cat.Event.OrganiserId != CallerId) return Forbid();

            cat.CategoryName = d.CategoryName; cat.CategoryType = d.CategoryType;
            cat.DistanceKm = d.DistanceKm; cat.MinAge = d.MinAge; cat.MaxAge = d.MaxAge;
            cat.Price = d.Price; cat.MaxParticipants = d.MaxParticipants;
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Organiser: delete a category (blocked if participants are enrolled).</summary>
        [HttpDelete("{id}"), Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (cat == null) return NotFound();
            if (cat.Event.OrganiserId != CallerId) return Forbid();
            if (await db.Enrolments.AnyAsync(e => e.CategoryId == id))
                return Conflict("Category has enrolments and cannot be deleted.");
            db.Categories.Remove(cat);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
