using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Participant: personal race history across all completed events.
[SessionAuthorize(Roles.Participant)]
public class MyResultsController : AppController
{
    private readonly ApiService _api;

    public MyResultsController(ApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<List<ResultDto>>("api/results/my");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success || result.Data == null)
        {
            ViewBag.Error = result.Error ?? "We could not load your results right now.";
            return View(new List<ResultDto>());
        }

        return View(result.Data);
    }
}