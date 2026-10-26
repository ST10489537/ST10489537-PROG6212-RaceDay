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