using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// The logged-in user's profile as returned by the API.
public class UserProfileDto
{
    public int UserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// The fields a user may change on their own profile.
public class UpdateProfileDto
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [MaxLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "The phone number can be at most 20 characters.")]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
}