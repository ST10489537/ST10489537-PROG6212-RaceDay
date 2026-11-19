using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// A Participant's request to enter an event (the event ID goes in the URL).
public class EnrolRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    public int CategoryId { get; set; }
}

// An enrolment as returned by the Part 2 API.
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