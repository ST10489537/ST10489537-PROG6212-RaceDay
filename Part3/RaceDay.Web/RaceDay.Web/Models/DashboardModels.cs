namespace RaceDay.Web.Models;

// Data for the Organiser dashboard. Totals are worked out from the events the
// API returns (each event already carries its enrolment count).
public class OrganiserDashboardViewModel
{
    public List<EventDto> Events { get; set; } = new();

    public int TotalEvents => Events.Count;

    public int UpcomingCount => Events.Count(e => e.EventDate >= DateTime.Now);

    public int TotalEnrolments => Events.Sum(e => e.EnrolmentCount);

    public EventDto? NextEvent => Events
        .Where(e => e.EventDate >= DateTime.Now)
        .OrderBy(e => e.EventDate)
        .FirstOrDefault();
}