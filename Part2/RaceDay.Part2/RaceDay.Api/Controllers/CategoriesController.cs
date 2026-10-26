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
    [SessionAuthorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public CategoriesController(RaceDayDbContext db)
        {
            _db = db;
        }

        private static readonly Expression<Func<Category, CategoryDto>> ToDto = c => new CategoryDto
        {
            CategoryID = c.CategoryID,
            EventID = c.EventID,
            Name = c.Name,
            Description = c.Description
        };

        /// <summary>Lists the categories of an event. Available to both roles.</summary>
        /// <param name="eventId">The event ID.</param>
        /// <response code="200">The event's categories.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="404">Event does not exist.</response>
        [HttpGet("api/events/{eventId:int}/categories")]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
                return NotFound(new { message = "Event not found." });

            var categories = await _db.Categories
                .Where(c => c.EventID == eventId)
                .OrderBy(c => c.Name)
                .Select(ToDto)
                .ToListAsync();
            return Ok(categories);
        }

        /// <summary>Adds a category to one of the Organiser's own events.</summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="dto">Category name and optional description.</param>
        /// <response code="201">Category created.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Event does not exist.</response>
        [HttpPost("api/events/{eventId:int}/categories")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var ev = await _db.Events.FirstOrDefaultAsync(e => e.EventID == eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only add categories to your own events." });

            var category = new Category
            {
                EventID = eventId,
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var created = await _db.Categories.Where(c => c.CategoryID == category.CategoryID).Select(ToDto).FirstAsync();
            return CreatedAtAction(nameof(GetCategories), new { eventId }, created);
        }

        /// <summary>Updates a category on one of the Organiser's own events.</summary>
        /// <param name="id">The category ID.</param>
        /// <param name="dto">The new name and description.</param>
        /// <response code="200">Category updated.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Category does not exist.</response>
        [HttpPut("api/categories/{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequestDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) return NotFound(new { message = "Category not found." });
            if (category.Event.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only change categories on your own events." });

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;
            await _db.SaveChangesAsync();

            var updated = await _db.Categories.Where(c => c.CategoryID == id).Select(ToDto).FirstAsync();
            return Ok(updated);
        }

        /// <summary>Deletes a category from one of the Organiser's own events, provided nobody has enrolled in it.</summary>
        /// <param name="id">The category ID.</param>
        /// <response code="204">Category deleted.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser, or the event belongs to another Organiser.</response>
        /// <response code="404">Category does not exist.</response>
        /// <response code="409">Participants have already enrolled in this category.</response>
        [HttpDelete("api/categories/{id:int}")]
        [SessionAuthorize("Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) return NotFound(new { message = "Category not found." });
            if (category.Event.OrganiserID != userId)
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only delete categories on your own events." });

            if (await _db.Enrolments.AnyAsync(e => e.CategoryID == id))
                return Conflict(new { message = "Participants have enrolled in this category, so it cannot be deleted." });

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}