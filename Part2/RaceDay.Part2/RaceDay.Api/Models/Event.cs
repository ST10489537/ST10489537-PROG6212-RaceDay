
using System;
using System.Collections.Generic;

namespace RaceDay.Api.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public EventType EventType { get; set; }
        public string? BannerImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User Organiser { get; set; } = null!;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }

   
}
