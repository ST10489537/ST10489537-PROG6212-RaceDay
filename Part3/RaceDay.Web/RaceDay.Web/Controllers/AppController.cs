using System.Net;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Base class for controllers that call the API.
public abstract class AppController : Controller
{
    // If the API says we are not logged in, send the user to Login with a
    // message. Returns null when everything is fine.
    protected IActionResult? SessionExpired<T>(ApiResult<T> result)
    {
        if (result.StatusCode != HttpStatusCode.Unauthorized) return null;

        TempData["Error"] = "Your session has expired. Please log in again.";
        return RedirectToAction("Login", "Account");
    }
}