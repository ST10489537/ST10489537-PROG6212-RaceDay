using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs
{
    /// <summary>Captures a result; the enrolment comes from the route.</summary>
    public class ResultRequestDto
    {
        /// <summary>Finish time as hh:mm:ss, e.g. 03:45:12.</summary>
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }
    }

    /// <summary>Changes an existing result.</summary>
    public class ResultUpdateDto
    {
        public TimeSpan FinishTime { get; set; }

        [Range(1, int.MaxValue)]
        public int FinishPosition { get; set; }
    }

    /// <summary>A result as returned by the API.</summary>
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
        /// <summary>Number of finishers recorded for the event (for "47th out of 312").</summary>
        public int TotalFinishers { get; set; }
        public DateTime CapturedAt { get; set; }
    }
}