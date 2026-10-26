using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [SessionAuthorize]   // any logged-in user (Organiser or Participant)
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _db;

        public ProfileController(RaceDayDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns the profile of the currently logged-in user.
        /// </summary>
        /// <response code="200">The user's profile (never includes the password).</response>
        /// <response code="401">Not logged in, or the account no longer exists.</response>
        [HttpGet]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return Unauthorized(new { message = "Your session is no longer valid. Please log in again." });
            }

            return Ok(ToProfile(user));
        }

        /// <summary>
        /// Updates the full name, phone number and profile picture URL of the currently logged-in user.
        /// Email and role cannot be changed here.
        /// </summary>
        /// <param name="dto">The new full name, phone number and profile picture URL.</param>
        /// <response code="200">The updated profile.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Not logged in, or the account no longer exists.</response>
        [HttpPut]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return Unauthorized(new { message = "Your session is no longer valid. Please log in again." });
            }

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber;
            user.ProfilePictureUrl = dto.ProfilePictureUrl;

            await _db.SaveChangesAsync();

            return Ok(ToProfile(user));
        }

        // Maps a User entity to the safe response DTO (never includes the password hash)
        private static UserProfileDto ToProfile(Models.User user) => new()
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = user.Role.RoleName,
            CreatedAt = user.CreatedAt
        };
    }
}