using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Participant: full event information, categories and the Enter Event action.
[SessionAuthorize(Roles.Participant)]
public class EventDetailsController : AppController
{
    private readonly ApiService _api;

    public EventDetailsController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        var result = await _api.GetAsync<EventDto>($"api/events/{id}");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We could not find that event.";
            return RedirectToAction("Index", "Home");
        }

        return View(result.Data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int id, EnrolRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please choose a category before entering the event.";
            return RedirectToAction(nameof(Index), new { id });
        }

        var result = await _api.PostForStatusAsync($"api/events/{id}/enrolments", model);

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "We could not enter you into this event.";
            return RedirectToAction(nameof(Index), new { id });
        }

        TempData["Success"] = "You are entered! Your enrolment is now pending confirmation.";
        return RedirectToAction("Index", "MyEvents");
    }
}