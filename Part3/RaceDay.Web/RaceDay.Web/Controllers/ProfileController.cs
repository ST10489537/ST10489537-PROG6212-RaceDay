using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Any logged-in user can view and update their own profile.
[SessionAuthorize]
public class ProfileController : AppController
{
    private readonly ApiService _api;

    public ProfileController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var result = await _api.GetAsync<UserProfileDto>("api/profile");

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "We could not load your profile right now.";
            return RedirectToAction("Index", "Home");
        }

        FillReadOnlyDetails(result.Data);
        return View(new UpdateProfileDto
        {
            FullName = result.Data.FullName,
            PhoneNumber = result.Data.PhoneNumber,
            ProfilePictureUrl = result.Data.ProfilePictureUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateProfileDto model, IFormFile? pictureFile)
    {
        if (!ModelState.IsValid)
        {
            await LoadReadOnlyDetailsAsync();
            return View(model);
        }

        // A new picture goes to the API, which stores it in Azure Blob Storage.
        if (pictureFile != null && pictureFile.Length > 0)
        {
            var problem = ImageRules.Validate(pictureFile);
            if (problem == null)
            {
                var upload = await _api.UploadImageAsync("api/uploads/profile-picture", pictureFile);

                var expiredUpload = SessionExpired(upload);
                if (expiredUpload != null) return expiredUpload;

                if (upload.Success && upload.Data != null)
                    model.ProfilePictureUrl = upload.Data.Url;
                else
                    problem = upload.Error ?? "We could not upload the picture. Please try again.";
            }

            if (problem != null)
            {
                await LoadReadOnlyDetailsAsync();
                ModelState.AddModelError(string.Empty, problem);
                return View(model);
            }
        }

        var result = await _api.PutForStatusAsync("api/profile", model);

        var expired = SessionExpired(result);
        if (expired != null) return expired;

        if (!result.Success)
        {
            await LoadReadOnlyDetailsAsync();
            ModelState.AddModelError(string.Empty,
                result.Error ?? "We could not save your profile. Please try again.");
            return View(model);
        }

        // Keep the name in the navbar up to date.
        HttpContext.Session.SetString(SessionKeys.FullName, model.FullName);

        TempData["Success"] = "Your profile was updated.";
        return RedirectToAction(nameof(Index));
    }

    // Email, role and member-since are shown but cannot be changed here.
    private void FillReadOnlyDetails(UserProfileDto profile)
    {
        ViewBag.Email = profile.Email;
        ViewBag.Role = profile.Role;
        ViewBag.MemberSince = profile.CreatedAt.ToString("dd MMM yyyy");
    }

    private async Task LoadReadOnlyDetailsAsync()
    {
        var result = await _api.GetAsync<UserProfileDto>("api/profile");
        if (result.Success && result.Data != null) FillReadOnlyDetails(result.Data);
    }
}