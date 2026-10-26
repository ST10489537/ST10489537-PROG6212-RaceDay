using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    /// <summary>Fields a user is allowed to change on their own profile.</summary>
    public class UpdateProfileDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }
    }
}