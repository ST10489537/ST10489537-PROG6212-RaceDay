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
    [Route("api/events")]
    [SessionAuthorize]   // every action needs a logged-in user; some also need the Organiser role
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public EventsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private static readonly Expression<Func<Event, EventDto>> ToDto = e => new EventDto
        {
            EventID = e.EventID,
            OrganiserID = e.OrganiserID,
            OrganiserName = e.Organiser.FullName,
            Name = e.Name,
            Description = e.Description,
            EventDate = e.EventDate,
            Location = e.Location,
            DistanceKm = e.DistanceKm,
            EventType = e.EventType,
            BannerImageUrl = e.BannerImageUrl,
            EnrolmentCount = e.Enrolments.Count,
            Categories = e.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    EventID = c.EventID,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToList()
        };

        /// <summary>Lists upcoming events, optionally filtered by event type. Available to both roles.</summary>
        /// <param name="type">Optional: Run, Walk or Cycle.</param>
        /// <param name="upcoming">Defaults to true (only events from today onwards). Pass false to include past events.</param>
        /// <response code="200">The matching events, soonest first.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEvents([FromQuery] EventType? type, [FromQuery] bool upcoming = true)
        {
            var query = _db.Events.AsQueryable();

            if (type.HasValue)
                query = query.Where(e => e.EventType == type.Value);
            if (upcoming)
                query = query.Where(e => e.EventDate >= DateTime.Today);

            var events = await query.OrderBy(e => e.EventDate).Select(ToDto).ToListAsync();
            return Ok(events);
        }

        /// <summary>Lists every event (past and upcoming) created by the logged-in Organiser, for their dashboard.</summary>
        /// <response code="200">The Organiser's own events.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser.</response>
        [HttpGet("mine")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(List<EventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var events = await _db.Events
                .Where(e => e.OrganiserID == userId)
                .OrderBy(e => e.EventDate)
                .Select(ToDto)
                .ToListAsync();
            return Ok(events);
        }

        /// <summary>Returns full detail for one event, including its categories. Available to both roles.</summary>
        /// <param name="id">The event ID.</param>
        /// <response code="200">The event with its categories.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">Event does not exist.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _db.Events.Where(e => e.EventID == id).Select(ToDto).FirstOrDefaultAsync();
            if (ev == null) return NotFound(new { message = "Event not found." });
            return Ok(ev);
        }

        /// <summary>Creates an event owned by the logged-in Organiser.</summary>
        /// <param name="dto">Name, description, date, location, distance, event type and optional banner image URL.</param>
        /// <response code="201">Event created.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser.</response>
        [HttpPost]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent([FromBody] EventRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = new Event
            {
                OrganiserID = userId,
                Name = dto.Name.Trim(),
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location.Trim(),
                DistanceKm = dto.DistanceKm,
                EventType = dto.EventType,
                BannerImageUrl = dto.BannerImageUrl
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            var created = await _db.Events.Where(e => e.EventID == ev.EventID).Select(ToDto).FirstAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventID }, created);
        }

        /// <summary>Updates an event. Only the Organiser who created it may do this.</summary>
        /// <param name="id">The event ID.</param>
        /// <param name="dto">The new event details.</param>
        /// <response code="200">Event updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Event does not exist.</response>
        [HttpPut("{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change your own events." });

            ev.Name = dto.Name.Trim();
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location.Trim();
            ev.DistanceKm = dto.DistanceKm;
            ev.EventType = dto.EventType;
            ev.BannerImageUrl = dto.BannerImageUrl;

            await _db.SaveChangesAsync();

            var updated = await _db.Events.Where(e => e.EventID == id).Select(ToDto).FirstAsync();
            return Ok(updated);
        }

        /// <summary>Deletes an event (and its categories). Only the owning Organiser may do this, and only if nobody has enrolled.</summary>
        /// <param name="id">The event ID.</param>
        /// <response code="204">Event deleted.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Event does not exist.</response>
        /// <response code="409">The event already has enrolments.</response>
        [HttpDelete("{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only delete your own events." });

            if (await _db.Enrolments.AnyAsync(e => e.EventID == id))
                return Conflict(new { message = "This event already has enrolments and cannot be deleted." });

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}