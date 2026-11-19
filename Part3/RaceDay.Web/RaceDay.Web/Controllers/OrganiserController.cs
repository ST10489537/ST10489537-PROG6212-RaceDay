using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Organiser-only pages. The filter blocks anyone who is not a logged-in Organiser.
[SessionAuthorize(Roles.Organiser)]
public class OrganiserController : AppController
{
    private readonly ApiService _api;

    public OrganiserController(ApiService api)
    {
        _api = api;
    }

    // ---------- Dashboard ----------

    // Summary of every event this Organiser has created.
    public async Task<IActionResult> Dashboard()
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events/mine");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        var model = new OrganiserDashboardViewModel();
        if (result.Success && result.Data != null)
        {
            model.Events = result.Data.OrderBy(e => e.EventDate).ToList();
        }
        else
        {
            ViewBag.Error = result.Error ?? "We could not load your events right now.";
        }

        return View(model);
    }

    // ---------- Events ----------

    // List of the Organiser's events with Edit and Delete buttons.
    public async Task<IActionResult> Events()
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events/mine");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success || result.Data == null)
        {
            ViewBag.Error = result.Error ?? "We could not load your events right now.";
            return View(new List<EventDto>());
        }

        return View(result.Data.OrderBy(e => e.EventDate).ToList());
    }

    [HttpGet]
    public IActionResult CreateEvent() => View(new EventRequestDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventRequestDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PostForStatusAsync("api/events", model);

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error ?? "We could not create the event. Please try again.");
            return View(model);
        }

        TempData["Success"] = $"The event \"{model.Name}\" was created.";
        return RedirectToAction(nameof(Events));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var result = await _api.GetAsync<EventDto>($"api/events/{id}");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We could not find that event.";
            return RedirectToAction(nameof(Events));
        }

        // Organisers may only edit their own events.
        if (result.Data.OrganiserID != HttpContext.Session.GetInt32(SessionKeys.UserId))
            return RedirectToAction("AccessDenied", "Account");

        var ev = result.Data;
        ViewBag.EventId = id;
        return View(new EventRequestDto
        {
            Name = ev.Name,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType,
            BannerImageUrl = ev.BannerImageUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, EventRequestDto model)
    {
        ViewBag.EventId = id;
        if (!ModelState.IsValid) return View(model);

        var result = await _api.PutForStatusAsync($"api/events/{id}", model);

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error ?? "We could not save your changes. Please try again.");
            return View(model);
        }

        TempData["Success"] = $"The event \"{model.Name}\" was updated.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _api.DeleteForStatusAsync($"api/events/{id}");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        // The API refuses to delete an event that already has enrolments;
        // its message is shown to the Organiser.
        if (result.Success)
            TempData["Success"] = "The event was deleted.";
        else
            TempData["Error"] = result.Error ?? "We could not delete that event.";

        return RedirectToAction(nameof(Events));
    }
}