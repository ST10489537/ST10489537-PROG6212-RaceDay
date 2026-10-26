using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(RaceDayDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }


        [HttpPost("register")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Role must be one of the two valid roles
            string roleName;
            if (string.Equals(dto.Role, "Organiser", StringComparison.OrdinalIgnoreCase))
                roleName = "Organiser";
            else if (string.Equals(dto.Role, "Participant", StringComparison.OrdinalIgnoreCase))
                roleName = "Participant";
            else
                return BadRequest(new { message = "Role must be either 'Organiser' or 'Participant'." });

            // 2. Organisers need the secret invite code
            if (roleName == "Organiser")
            {
                var requiredCode = _config["OrganiserSettings:InviteCode"];
                if (string.IsNullOrEmpty(requiredCode) || dto.OrganiserCode != requiredCode)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new { message = "A valid organiser invite code is required to register as an Organiser." });
            }

            // 3. Email must be unique
            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return Conflict(new { message = "An account with that email already exists." });

            // 4. Look up the role row, then create the user with a HASHED password
            var role = await _db.Roles.FirstAsync(r => r.RoleName == roleName);

            var user = new User
            {
                RoleID = role.RoleID,
                FullName = dto.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                PhoneNumber = dto.PhoneNumber
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, ToProfile(user, roleName));
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Same message for "no such user" and "wrong password" so attackers can't tell which was wrong
            if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            // Start a fresh session, then store who the user is and what role they have
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            return Ok(ToProfile(user, user.Role.RoleName));
        }


        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });
        }

        // Maps a User entity to the safe response DTO (never includes the password hash)
        private static UserProfileDto ToProfile(User user, string roleName) => new()
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = roleName,
            CreatedAt = user.CreatedAt
        };
    }
}