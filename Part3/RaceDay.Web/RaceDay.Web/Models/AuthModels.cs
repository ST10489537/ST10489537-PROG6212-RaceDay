using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// Sent to the API's login endpoint.
public class LoginDto
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

// Sent to the API's register endpoint (matches the Part 2 contract).
public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? OrganiserCode { get; set; }
}

// What the Register form binds to. It adds ConfirmPassword, which the API
// does not need, so it is mapped onto RegisterDto before sending.
public class RegisterViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [MaxLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "The phone number can be at most 20 characters.")]
    [Display(Name = "Phone number (optional)")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Please choose a role.")]
    [Display(Name = "I am registering as a")]
    public string Role { get; set; } = "Participant";

    [Display(Name = "Organiser invite code")]
    public string? OrganiserCode { get; set; }

    [Required(ErrorMessage = "Please enter a password.")]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters.")]
    [MaxLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}