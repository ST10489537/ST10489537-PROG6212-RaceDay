namespace RaceDay.Web.Models;

// Data for the Organiser's category page: the event plus the "add" form.
public class CategoriesViewModel
{
    public EventDto Event { get; set; } = new();
    public CategoryRequestDto NewCategory { get; set; } = new();
}

// Data for the Organiser's enrolments page.
public class EventEnrolmentsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}

// One line on the results page: an enrolment and its result, if captured.
public class ResultRowViewModel
{
    public EnrolmentDto Enrolment { get; set; } = new();
    public ResultDto? Result { get; set; }
}

// Data for the Organiser's results entry page.
public class EventResultsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<ResultRowViewModel> Rows { get; set; } = new();
}

// Data for the Participant home page (events plus the active filters).
public class ParticipantHomeViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public EventType? Type { get; set; }
    public string? Search { get; set; }
}