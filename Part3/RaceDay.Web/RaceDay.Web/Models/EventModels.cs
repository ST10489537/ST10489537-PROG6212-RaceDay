using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// An event as returned by the Part 2 API.
public class EventDto
{
    public int EventID { get; set; }
    public int OrganiserID { get; set; }
    public string OrganiserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public string? BannerImageUrl { get; set; }
    public int EnrolmentCount { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

// Fields an Organiser fills in to create or edit an event.
// Validation mirrors the API rules so mistakes are caught on the form.
public class EventRequestDto
{
    [Required(ErrorMessage = "Please enter the event name.")]
    [MaxLength(150, ErrorMessage = "The name can be at most 150 characters.")]
    [Display(Name = "Event name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "The description can be at most 1000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose the event date and time.")]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    [Display(Name = "Date and time")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

    [Required(ErrorMessage = "Please enter the location.")]
    [MaxLength(200, ErrorMessage = "The location can be at most 200 characters.")]
    public string Location { get; set; } = string.Empty;

    [Range(0.01, 999.99, ErrorMessage = "Distance must be between 0.01 and 999.99 km.")]
    [Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; }

    [Display(Name = "Event type")]
    public EventType EventType { get; set; }

    // Filled in by the banner upload (added in the Azure Blob phase).
    [MaxLength(500)]
    public string? BannerImageUrl { get; set; }
}