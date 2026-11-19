using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : AppController
{
    private readonly ApiService _api;

    public HomeController(ApiService api)
    {
        _api = api;
    }

    // Participants see upcoming events with filters. Organisers go to their
    // dashboard, and visitors who are not logged in go to Login.
    public async Task<IActionResult> Index(EventType? type, string? search)
    {
        var session = HttpContext.Session;

        if (session.GetInt32(SessionKeys.UserId) == null)
            return RedirectToAction("Login", "Account");

        if (session.GetString(SessionKeys.Role) == Roles.Organiser)
            return RedirectToAction("Dashboard", "Organiser");

        var result = await _api.GetAsync<List<EventDto>>("api/events");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        var model = new ParticipantHomeViewModel { Type = type, Search = search };

        if (!result.Success || result.Data == null)
        {
            ViewBag.Error = result.Error ?? "We could not load the events right now.";
            return View(model);
        }

        var events = result.Data.AsEnumerable();

        if (type.HasValue)
            events = events.Where(e => e.EventType == type.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            events = events.Where(e =>
                e.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        model.Events = events.OrderBy(e => e.EventDate).ToList();
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}