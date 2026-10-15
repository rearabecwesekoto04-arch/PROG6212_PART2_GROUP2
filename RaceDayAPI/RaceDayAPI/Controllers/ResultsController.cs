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
    [ApiController, Route("api/results"), Authorize]
    public class ResultsController(AppDbContext db) : ControllerBase
    {
        int CallerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Organiser: capture a finish time and position for an enrolment.</summary>
        [HttpPost, Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(ResultDto d)
        {
            var enrol = await db.Enrolments.Include(e => e.Category).ThenInclude(c => c.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == d.EnrolmentId);
            if (enrol == null) return NotFound("Enrolment not found.");
            if (enrol.Category.Event.OrganiserId != CallerId) return Forbid();
            if (await db.Results.AnyAsync(r => r.EnrolmentId == d.EnrolmentId))
                return Conflict("A result already exists for this enrolment. Use PUT to update it.");

            var res = new Result
            {
                EnrolmentId = d.EnrolmentId,
                FinishTime = d.FinishTime,
                Position = d.Position,
                Status = d.Status
            };
            db.Results.Add(res);
            await db.SaveChangesAsync();
            return StatusCode(201, new { res.ResultId });
        }

        /// <summary>Organiser: update an existing result.</summary>
        [HttpPut("{id}"), Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(int id, ResultDto d)
        {
            var res = await db.Results.Include(r => r.Enrolment).ThenInclude(e => e.Category)
                .ThenInclude(c => c.Event).FirstOrDefaultAsync(r => r.ResultId == id);
            if (res == null) return NotFound();
            if (res.Enrolment.Category.Event.OrganiserId != CallerId) return Forbid();

            res.FinishTime = d.FinishTime; res.Position = d.Position; res.Status = d.Status;
            await db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Organiser: view all results for one of their events.</summary>
        [HttpGet("event/{eventId}"), Authorize(Roles = "Organiser")]
        public async Task<IActionResult> ForEvent(int eventId)
        {
            var ev = await db.Events.FindAsync(eventId);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != CallerId) return Forbid();

            return Ok(await db.Results.Where(r => r.Enrolment.Category.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new {
                    r.ResultId,
                    r.EnrolmentId,
                    r.FinishTime,
                    r.Position,
                    r.Status,
                    Participant = r.Enrolment.Participant.FullName,
                    Category = r.Enrolment.Category.CategoryName
                })
                .ToListAsync());
        }

        /// <summary>Participant: view their own results.</summary>
        [HttpGet("mine"), Authorize(Roles = "Participant")]
        public async Task<IActionResult> Mine() =>
            Ok(await db.Results.Where(r => r.Enrolment.ParticipantId == CallerId)
                .Select(r => new {
                    r.ResultId,
                    r.FinishTime,
                    r.Position,
                    r.Status,
                    Event = r.Enrolment.Category.Event.EventName,
                    Category = r.Enrolment.Category.CategoryName
                })
                .ToListAsync());
    }
}
