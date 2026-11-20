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
    public async Task<IActionResult> CreateEvent(EventRequestDto model, IFormFile? bannerFile)
    {
        if (!ModelState.IsValid) return View(model);

        // Upload the banner (through the API) before saving the event.
        if (!await TryAttachBannerAsync(model, bannerFile)) return View(model);

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
    public async Task<IActionResult> EditEvent(int id, EventRequestDto model, IFormFile? bannerFile)
    {
        ViewBag.EventId = id;
        if (!ModelState.IsValid) return View(model);

        // Upload a new banner (through the API) if one was chosen.
        if (!await TryAttachBannerAsync(model, bannerFile)) return View(model);

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

    // ---------- Helpers ----------

    // Sends the chosen banner to the API (never to Azure directly) and stores
    // the returned URL on the model. Returns false if the image was rejected.
    private async Task<bool> TryAttachBannerAsync(EventRequestDto model, IFormFile? file)
    {
        // No new file chosen: keep whatever banner URL the event already has.
        if (file == null || file.Length == 0) return true;

        var problem = ImageRules.Validate(file);
        if (problem != null)
        {
            ModelState.AddModelError(string.Empty, problem);
            return false;
        }

        var upload = await _api.UploadImageAsync("api/uploads/event-banner", file);
        if (!upload.Success || upload.Data == null)
        {
            ModelState.AddModelError(string.Empty,
                upload.Error ?? "We could not upload the banner image. Please try again.");
            return false;
        }

        model.BannerImageUrl = upload.Data.Url;
        return true;
    }
}