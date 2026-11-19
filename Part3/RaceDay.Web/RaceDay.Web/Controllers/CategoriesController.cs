using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Organiser: add, edit and delete the categories of one of their events.
[SessionAuthorize(Roles.Organiser)]
public class CategoriesController : OrganiserBaseController
{
    public CategoriesController(ApiService api) : base(api) { }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        return View(new CategoriesViewModel { Event = ev! });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int eventId,
        [Bind(Prefix = "NewCategory")] CategoryRequestDto model)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        model ??= new CategoryRequestDto();

        if (ModelState.IsValid)
        {
            var result = await Api.PostForStatusAsync($"api/events/{eventId}/categories", model);

            var expired = SessionExpired(result);
            if (expired != null) return expired;

            if (result.Success)
            {
                TempData["Success"] = $"The category \"{model.Name}\" was added.";
                return RedirectToAction(nameof(Index), new { eventId });
            }

            ModelState.AddModelError(string.Empty,
                result.Error ?? "We could not add the category. Please try again.");
        }

        return View(nameof(Index), new CategoriesViewModel { Event = ev!, NewCategory = model });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int eventId, int id)
    {
        var (ev, redirect) = await LoadOwnEventAsync(eventId);
        if (redirect != null) return redirect;

        var category = ev!.Categories.FirstOrDefault(c => c.CategoryID == id);
        if (category == null)
        {
            TempData["Error"] = "We could not find that category.";
            return RedirectToAction(nameof(Index), new { eventId });
        }

        ViewBag.EventId = eventId;
        ViewBag.CategoryId = id;
        ViewBag.EventName = ev.Name;
        return View(new CategoryRequestDto { Name = category.Name, Description = category.Description });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int eventId, int id, CategoryRequestDto model)
    {
        ViewBag.EventId = eventId;
        ViewBag.CategoryId = id;
        ViewBag.EventName = string.Empty;

        if (!ModelState.IsValid) return View(model);

        var result = await Api.PutForStatusAsync($"api/categories/{id}", model);

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error ?? "We could not save your changes. Please try again.");
            return View(model);
        }

        TempData["Success"] = $"The category \"{model.Name}\" was updated.";
        return RedirectToAction(nameof(Index), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int eventId, int id)
    {
        var result = await Api.DeleteForStatusAsync($"api/categories/{id}");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        // The API refuses to delete a category that already has enrolments.
        if (result.Success)
            TempData["Success"] = "The category was deleted.";
        else
            TempData["Error"] = result.Error ?? "We could not delete that category.";

        return RedirectToAction(nameof(Index), new { eventId });
    }
}