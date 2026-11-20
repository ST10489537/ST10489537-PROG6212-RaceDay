namespace RaceDay.Web.Helpers;

// Quick checks on a chosen image before it is sent to the API.
public static class ImageRules
{
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxBytes = 5 * 1024 * 1024;

    // Returns a friendly message if the file is not acceptable, otherwise null.
    public static string? Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(extension)) return "Please choose a JPG, PNG or WEBP image.";
        if (file.Length > MaxBytes) return "The image must be 5 MB or smaller.";

        return null;
    }
}