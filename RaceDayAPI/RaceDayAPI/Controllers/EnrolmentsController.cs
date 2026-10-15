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
    [ApiController, Route("api/enrolments"), Authorize]
    public class EnrolmentsController(AppDbContext db) : ControllerBase
    {
        int CallerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Participant: enter an event by selecting a category.</summary>
        [HttpPost, Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(EnrolDto dto)
        {
            var cat = await db.Categories.FindAsync(dto.CategoryId);
            if (cat == null) return NotFound("Category not found.");

            // one enrolment per event per participant
            bool already = await db.Enrolments.AnyAsync(e =>
                e.ParticipantId == CallerId && e.Category.EventId == cat.EventId);
            if (already) return Conflict("You are already enrolled in this event.");

            if (cat.MaxParticipants != null)
            {
                int taken = await db.Enrolments.CountAsync(e => e.CategoryId == cat.CategoryId);
                if (taken >= cat.MaxParticipants) return Conflict("This category is full.");
            }

            var enrol = new Enrolment { ParticipantId = CallerId, CategoryId = cat.CategoryId };
            db.Enrolments.Add(enrol);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(Mine), new { id = enrol.EnrolmentId },
                new { enrol.EnrolmentId, enrol.CategoryId, enrol.Status });
        }

        /// <summary>Participant: view their own enrolments.</summary>
        [HttpGet("mine"), Authorize(Roles = "Participant")]
        public async Task<IActionResult> Mine() =>
            Ok(await db.Enrolments.Where(e => e.ParticipantId == CallerId)
                .Select(e => new {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.Status,
                    Category = e.Category.CategoryName,
                    EventId = e.Category.EventId,
                    Event = e.Category.Event.EventName
                })
                .ToListAsync());

        /// <summary>Organiser: view all enrolments for one of their events.</summary>
        [HttpGet("event/{eventId}"), Authorize(Roles = "Organiser")]
        public async Task<IActionResult> ForEvent(int eventId)
        {
            var ev = await db.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CallerId) return Forbid();

            return Ok(await db.Enrolments.Where(e => e.Category.EventId == eventId)
                .Select(e => new {
                    e.EnrolmentId,
                    e.EnrolmentDate,
                    e.Status,
                    Participant = e.Participant.FullName,
                    ParticipantEmail = e.Participant.Email,
                    Category = e.Category.CategoryName
                })
                .ToListAsync());
        }
    }
}
