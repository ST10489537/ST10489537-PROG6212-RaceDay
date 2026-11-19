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
}