using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.DTOs
{
    /// <summary>Fields an Organiser supplies to create or update an event.</summary>
    public class EventRequestDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }


        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 999.99)]
        public decimal DistanceKm { get; set; }

        /// <summary>Run, Walk or Cycle.</summary>
        public EventType EventType { get; set; }

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }
    }


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
}