using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Participant: every event they have entered, with its status.
[SessionAuthorize(Roles.Participant)]
public class MyEventsController : AppController
{
    private readonly ApiService _api;

    public MyEventsController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<EnrolmentDto>>("api/enrolments/my");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success || result.Data == null)
        {
            ViewBag.Error = result.Error ?? "We could not load your events right now.";
            return View(new List<EnrolmentDto>());
        }

        return View(result.Data);
    }
}