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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ResultsController(RaceDayDbContext db)
        {
            _db = db;
        }

        private static readonly Expression<Func<Result, ResultDto>> ToDto = r => new ResultDto
        {
            ResultID = r.ResultID,
            EnrolmentID = r.EnrolmentID,
            ParticipantName = r.Enrolment.Participant.FullName,
            EventID = r.Enrolment.EventID,
            EventName = r.Enrolment.Event.Name,
            EventDate = r.Enrolment.Event.EventDate,
            CategoryName = r.Enrolment.Category.Name,
            FinishTime = r.FinishTime,
            FinishPosition = r.FinishPosition,
            TotalFinishers = r.Enrolment.Event.Enrolments.Count(x => x.Result != null),
            CapturedAt = r.CapturedAt
        };

        /// <summary>Captures the finish time and position for an enrolment in one of the Organiser's own events.</summary>
        /// <param name="enrolmentId">The enrolment the result belongs to.</param>
        /// <param name="dto">Finish time (hh:mm:ss) and finishing position.</param>
        /// <response code="201">Result captured.</response>
        /// <response code="400">Validation failed, or the enrolment was cancelled.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Enrolment does not exist.</response>
        /// <response code="409">A result already exists for this enrolment.</response>
        [HttpPost("api/enrolments/{enrolmentId:int}/results")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CaptureResult(int enrolmentId, [FromBody] ResultRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var enrolment = await _db.Enrolments.Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);
            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only capture results for your own events." });
            if (enrolment.Status == EnrolmentStatus.Cancelled)
                return BadRequest(new { message = "A cancelled enrolment cannot receive a result." });
            if (await _db.Results.AnyAsync(r => r.EnrolmentID == enrolmentId))
                return Conflict(new { message = "A result has already been captured for this enrolment." });

            var result = new Result
            {
                EnrolmentID = enrolmentId,
                FinishTime = dto.FinishTime,
                FinishPosition = dto.FinishPosition
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            var created = await _db.Results.Where(r => r.ResultID == result.ResultID).Select(ToDto).FirstAsync();
            return StatusCode(StatusCodes.Status201Created, created);
        }

        /// <summary>Corrects an existing result for one of the Organiser's own events.</summary>
        /// <param name="id">The result ID.</param>
        /// <param name="dto">The new finish time and position.</param>
        /// <response code="200">Result updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Result does not exist.</response>
        [HttpPut("api/results/{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultUpdateDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var result = await _db.Results
                .Include(r => r.Enrolment).ThenInclude(e => e.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);
            if (result == null) return NotFound(new { message = "Result not found." });
            if (result.Enrolment.Event.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change results for your own events." });

            result.FinishTime = dto.FinishTime;
            result.FinishPosition = dto.FinishPosition;
            await _db.SaveChangesAsync();

            var updated = await _db.Results.Where(r => r.ResultID == id).Select(ToDto).FirstAsync();
            return Ok(updated);
        }

        /// <summary>Lists all captured results for one of the Organiser's own events, in finishing order.</summary>
        /// <param name="eventId">The event ID.</param>
        /// <response code="200">The event's results.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Event does not exist.</response>
        [HttpGet("api/events/{eventId:int}/results")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only view results for your own events." });

            var results = await _db.Results
                .Where(r => r.Enrolment.EventID == eventId)
                .OrderBy(r => r.FinishPosition)
                .Select(ToDto)
                .ToListAsync();
            return Ok(results);
        }

        /// <summary>Lists the logged-in Participant's own results across all events, most recent first.</summary>
        /// <response code="200">The Participant's race history.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not a Participant.</response>
        [HttpGet("api/results/my")]
        [SessionAuthorize("Participant")]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var results = await _db.Results
                .Where(r => r.Enrolment.ParticipantID == userId)
                .OrderByDescending(r => r.Enrolment.Event.EventDate)
                .Select(ToDto)
                .ToListAsync();
            return Ok(results);
        }
    }
}