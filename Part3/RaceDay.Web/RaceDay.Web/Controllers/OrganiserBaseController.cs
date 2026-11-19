using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Shared behaviour for Organiser pages that work on one event.
public abstract class OrganiserBaseController : AppController
{
    protected readonly ApiService Api;

    protected OrganiserBaseController(ApiService api)
    {
        Api = api;
    }

    // Loads an event and checks it belongs to the logged-in Organiser.
    // Returns either the event, or a redirect to send the user away.
    protected async Task<(EventDto? Event, IActionResult? Redirect)> LoadOwnEventAsync(int eventId)
    {
        var result = await Api.GetAsync<EventDto>($"api/events/{eventId}");

        var expired = SessionExpired(result);
        if (expired != null) return (null, expired);

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We could not find that event.";
            return (null, RedirectToAction("Events", "Organiser"));
        }

        if (result.Data.OrganiserID != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return (null, RedirectToAction("AccessDenied", "Account"));

        return (result.Data, null);
    }
}