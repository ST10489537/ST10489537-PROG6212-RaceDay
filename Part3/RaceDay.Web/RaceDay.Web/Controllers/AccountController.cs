using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Register, login and logout. All real work is done by the Part 2 API.
public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Already logged in? Skip the form.
        if (HttpContext.Session.GetInt32(SessionKeys.UserId) != null)
            return RedirectToAction("Index", "Home");

        return View(new LoginDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        // 1. Ask the API to log in. It starts the API session and sends a cookie.
        var login = await _api.PostForStatusAsync("api/auth/login", model);
        if (!login.Success)
        {
            ModelState.AddModelError(string.Empty,
                login.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Incorrect email or password."
                    : login.Error ?? "We could not log you in. Please try again.");
            return View(model);
        }

        // 2. Ask the API who we are, so we know the user ID and role.
        var profile = await _api.GetAsync<UserProfileDto>("api/profile");
        if (!profile.Success || profile.Data == null)
        {
            _api.ClearApiSession();
            ModelState.AddModelError(string.Empty,
                "You were logged in but we could not load your profile. Please try again.");
            return View(model);
        }

        // 3. Remember the user in the MVC session.
        HttpContext.Session.SetInt32(SessionKeys.UserId, profile.Data.UserID);
        HttpContext.Session.SetString(SessionKeys.FullName, profile.Data.FullName);
        HttpContext.Session.SetString(SessionKeys.Role, profile.Data.Role);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32(SessionKeys.UserId) != null)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Organisers need an invite code (set in the API's appsettings.json).
        if (model.Role == Roles.Organiser && string.IsNullOrWhiteSpace(model.OrganiserCode))
        {
            ModelState.AddModelError(nameof(model.OrganiserCode),
                "Organisers must enter the invite code.");
        }

        if (!ModelState.IsValid) return View(model);

        var dto = new RegisterDto
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Password = model.Password,
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
            Role = model.Role,
            OrganiserCode = model.Role == Roles.Organiser ? model.OrganiserCode : null
        };

        var result = await _api.PostForStatusAsync("api/auth/register", dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty,
                result.Error ?? "We could not create your account. Please try again.");
            return View(model);
        }

        // Drop any API session the register call may have started, then ask
        // the user to log in so the flow is the same for everyone.
        _api.ClearApiSession();
        TempData["Success"] = "Your account has been created. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // End the API session, then clear ours.
        await _api.PostForStatusAsync("api/auth/logout");
        _api.ClearApiSession();
        HttpContext.Session.Clear();

        TempData["Success"] = "You have been logged out.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}