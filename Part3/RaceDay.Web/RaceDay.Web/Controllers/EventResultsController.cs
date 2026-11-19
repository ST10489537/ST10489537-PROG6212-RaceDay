using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Organiser: capture and correct finish times and positions for an event.
[SessionAuthorize(Roles.Organiser)]
public class EventResultsController : OrganiserBaseController
{
    public EventResultsController(ApiService api) : base(api) { }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        var enrolments = await Api.GetAsync<List<EnrolmentDto>>($"api/events/{eventId}/enrolments");
        var expired = SessionExpired(enrolments);
        if (expired != null) return expired;

        var results = await Api.GetAsync<List<ResultDto>>($"api/events/{eventId}/results");

        var model = new EventResultsViewModel { Event = ev! };

        if (!enrolments.Success || enrolments.Data == null)
        {
            ViewBag.Error = enrolments.Error ?? "We could not load the enrolments right now.";
            return View(model);
        }

        if (!results.Success)
            ViewBag.Error = "We could not load the saved results, so existing results may not show.";

        var byEnrolment = (results.Data ?? new List<ResultDto>())
            .ToDictionary(r => r.EnrolmentID);

        model.Rows = enrolments.Data
            .Where(e => e.Status != EnrolmentStatus.Cancelled)
            .OrderBy(e => e.ParticipantName)
            .Select(e => new ResultRowViewModel
            {
                Enrolment = e,
                Result = byEnrolment.GetValueOrDefault(e.EnrolmentID)
            })
            .ToList();

        return View(model);
    }

    // Creates a new result, or updates an existing one when resultId is given.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int eventId, int enrolmentId, int? resultId,
        string? finishTime, int finishPosition)
    {
        if (!TimeSpan.TryParseExact((finishTime ?? string.Empty).Trim(), @"hh\:mm\:ss",
                CultureInfo.InvariantCulture, out var time) || time <= TimeSpan.Zero)
        {
            TempData["Error"] = "Enter the finish time as hh:mm:ss, for example 03:45:12.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        if (finishPosition < 1)
        {
            TempData["Error"] = "The finishing position must be 1 or higher.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        var body = new ResultRequestDto { FinishTime = time, FinishPosition = finishPosition };

        var result = resultId.HasValue
            ? await Api.PutForStatusAsync($"api/results/{resultId.Value}", body)
            : await Api.PostForStatusAsync($"api/enrolments/{enrolmentId}/results", body);

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (result.Success)
            TempData["Success"] = "The result was saved.";
        else
            TempData["Error"] = result.Error ?? "We could not save that result.";

        return RedirectToAction(nameof(Index), new { eventId });
    }
}