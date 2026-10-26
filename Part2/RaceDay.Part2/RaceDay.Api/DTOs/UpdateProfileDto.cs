using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    
    public class UpdateProfileDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}