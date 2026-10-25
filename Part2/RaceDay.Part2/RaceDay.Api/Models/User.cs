
using System;
using System.Collections.Generic;

namespace RaceDay.Api.Models
{
    public class User
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Role Role { get; set; } = null!;
        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
