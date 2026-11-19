using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Organiser: see everyone who has enrolled for one of their events.
[SessionAuthorize(Roles.Organiser)]
public class EventEnrolmentsController : OrganiserBaseController
{
    public EventEnrolmentsController(ApiService api) : base(api) { }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        var result = await Api.GetAsync<List<EnrolmentDto>>($"api/events/{eventId}/enrolments");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        var model = new EventEnrolmentsViewModel { Event = ev! };
        if (result.Success && result.Data != null)
            model.Enrolments = result.Data.OrderBy(e => e.ParticipantName).ToList();
        else
            ViewBag.Error = result.Error ?? "We could not load the enrolments right now.";

        return View(model);
    }
}