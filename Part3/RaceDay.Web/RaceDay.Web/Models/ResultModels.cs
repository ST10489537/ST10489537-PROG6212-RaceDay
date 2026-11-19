using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// A result as returned by the API. TotalFinishers supports "47th out of 312".
public class ResultDto
{
    public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int FinishPosition { get; set; }
    public int TotalFinishers { get; set; }
    public DateTime CapturedAt { get; set; }
}

// Used by the Organiser's results form. The same shape works for capturing
// a new result and for correcting one (the API's update DTO has the same fields).
public class ResultRequestDto
{
    [Display(Name = "Finish time (hh:mm:ss)")]
    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Position must be 1 or higher.")]
    [Display(Name = "Finishing position")]
    public int FinishPosition { get; set; }
}