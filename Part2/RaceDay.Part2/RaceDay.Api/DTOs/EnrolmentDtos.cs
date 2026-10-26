using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.DTOs
{
    /// <summary>A Participant's request to enter an event; the event comes from the route.</summary>
    public class EnrolRequestDto
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

    /// <summary>An enrolment as returned by the API.</summary>
    public class EnrolmentDto
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public EnrolmentStatus Status { get; set; }
    }
}