using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EnrolmentsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private static readonly Expression<Func<Enrolment, EnrolmentDto>> ToDto = e => new EnrolmentDto
        {
            EnrolmentID = e.EnrolmentID,
            ParticipantID = e.ParticipantID,
            ParticipantName = e.Participant.FullName,
            EventID = e.EventID,
            EventName = e.Event.Name,
            EventDate = e.Event.EventDate,
            CategoryID = e.CategoryID,
            CategoryName = e.Category.Name,
            EnrolmentDate = e.EnrolmentDate,
            Status = e.Status
        };

        /// <summary>A Participant enters an event by choosing one of its categories. The enrolment starts as Pending.</summary>
        /// <param name="eventId">The event to enter.</param>
        /// <param name="dto">The chosen category ID.</param>
        /// <response code="201">Enrolment recorded.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not a Participant.</response>
        /// <response code="404">Event not found, or the category does not belong to that event.</response>
        /// <response code="409">The Participant is already enrolled in this event.</response>
        [HttpPost("api/events/{eventId:int}/enrolments")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
                return NotFound(new { message = "Event not found." });

            var categoryOk = await _db.Categories.AnyAsync(c => c.CategoryID == dto.CategoryId && c.EventID == eventId);
            if (!categoryOk)
                return NotFound(new { message = "That category does not exist for this event." });

            if (await _db.Enrolments.AnyAsync(e => e.ParticipantID == userId && e.EventID == eventId))
                return Conflict(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantID = userId,
                EventID = eventId,
                CategoryID = dto.CategoryId,
                Status = EnrolmentStatus.Pending
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            var created = await _db.Enrolments.Where(e => e.EnrolmentID == enrolment.EnrolmentID).Select(ToDto).FirstAsync();
            return CreatedAtAction(nameof(GetMyEnrolments), null, created);
        }

        /// <summary>Lists the logged-in Participant's own enrolments, newest first.</summary>
        /// <response code="200">The Participant's enrolments.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not a Participant.</response>
        [HttpGet("api/enrolments/my")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(typeof(List<EnrolmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var enrolments = await _db.Enrolments
                .Where(e => e.ParticipantID == userId)
                .OrderByDescending(e => e.EnrolmentDate)
                .Select(ToDto)
                .ToListAsync();
            return Ok(enrolments);
        }

        /// <summary>Lists everyone enrolled in one of the Organiser's own events.</summary>
        /// <param name="eventId">The event ID.</param>
        /// <response code="200">The event's enrolments.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Event does not exist.</response>
        [HttpGet("api/events/{eventId:int}/enrolments")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(List<EnrolmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view enrolments for your own events." });

            var enrolments = await _db.Enrolments
                .Where(e => e.EventID == eventId)
                .OrderBy(e => e.EnrolmentDate)
                .Select(ToDto)
                .ToListAsync();
            return Ok(enrolments);
        }
    }
}