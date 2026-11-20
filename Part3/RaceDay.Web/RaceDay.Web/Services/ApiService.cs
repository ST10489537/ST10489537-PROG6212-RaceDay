using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;

// Wraps the outcome of an API call so controllers never deal with raw HTTP.
public class ApiResult<T>
{
    public bool Success { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

// The only class in the MVC project that talks to the Part 2 API.
// It keeps each user's API session cookie inside their MVC session and
// sends it with every request, so the API can enforce roles.
public class ApiService
{
    // Key under which the API's cookies are stored in the MVC session.
    private const string CookieKey = "ApiSessionCookie";

    // The API sends enums as text (e.g. "Run"), so read them that way.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiService(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    // ----- Calls that return data -----

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Get, url);

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) =>
        SendAsync<T>(HttpMethod.Post, url, body);

    public Task<ApiResult<T>> PutAsync<T>(string url, object? body = null) =>
        SendAsync<T>(HttpMethod.Put, url, body);

    // ----- Calls where only success or failure matters -----

    public Task<ApiResult<string>> PostForStatusAsync(string url, object? body = null) =>
        SendRawAsync(HttpMethod.Post, url, body);

    public Task<ApiResult<string>> PutForStatusAsync(string url, object? body = null) =>
        SendRawAsync(HttpMethod.Put, url, body);

    public Task<ApiResult<string>> DeleteForStatusAsync(string url) =>
        SendRawAsync(HttpMethod.Delete, url);

    // Sends an image to one of the API's upload endpoints (multipart/form-data).
    // The MVC app never talks to Azure: the API stores the file and returns its URL.
    public async Task<ApiResult<UploadResultDto>> UploadImageAsync(string url, IFormFile file)
    {
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        var raw = await SendRawAsync(HttpMethod.Post, url, content: form);

        var result = new ApiResult<UploadResultDto>
        {
            Success = raw.Success,
            StatusCode = raw.StatusCode,
            Error = raw.Error
        };

        if (raw.Success && !string.IsNullOrWhiteSpace(raw.Data))
        {
            try
            {
                result.Data = JsonSerializer.Deserialize<UploadResultDto>(raw.Data, JsonOptions);
            }
            catch (JsonException)
            {
                result.Success = false;
                result.Error = "The server returned data in an unexpected format.";
            }
        }

        return result;
    }

    // Clears the stored API cookies (used on logout).
    public void ClearApiSession() =>
        _accessor.HttpContext?.Session.Remove(CookieKey);

    // ----- Internals -----

    // Sends the request, then converts the JSON response into type T.
    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        var raw = await SendRawAsync(method, url, body);

        var result = new ApiResult<T>
        {
            Success = raw.Success,
            StatusCode = raw.StatusCode,
            Error = raw.Error
        };

        if (raw.Success && !string.IsNullOrWhiteSpace(raw.Data))
        {
            try
            {
                result.Data = JsonSerializer.Deserialize<T>(raw.Data, JsonOptions);
            }
            catch (JsonException)
            {
                result.Success = false;
                result.Error = "The server returned data in an unexpected format.";
            }
        }

        return result;
    }

    private async Task<ApiResult<string>> SendRawAsync(HttpMethod method, string url,
    object? body = null, HttpContent? content = null)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            var session = _accessor.HttpContext?.Session;

            // Send the API session cookie so the API recognises the user.
            var cookie = session?.GetString(CookieKey);
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }

            if (content != null)
            {
                request.Content = content;
            }
            else if (body != null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
            }

            using var response = await _http.SendAsync(request);

            // Keep any cookies the API issues (its session cookie) in our session.
            if (session != null &&
                response.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                var jar = new Dictionary<string, string>();
                foreach (var part in (session.GetString(CookieKey) ?? string.Empty)
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var pair = part.Split('=', 2);
                    if (pair.Length == 2) jar[pair[0]] = pair[1];
                }

                foreach (var setCookie in setCookies)
                {
                    var pair = setCookie.Split(';')[0].Split('=', 2);
                    if (pair.Length == 2) jar[pair[0].Trim()] = pair[1].Trim();
                }

                session.SetString(CookieKey,
                    string.Join("; ", jar.Select(kv => $"{kv.Key}={kv.Value}")));
            }

            // If the API no longer recognises us (for example it restarted),
            // clear our session so the user is sent back to the login page.
            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                !url.EndsWith("auth/login", StringComparison.OrdinalIgnoreCase))
            {
                session?.Clear();
            }

            var text = await response.Content.ReadAsStringAsync();

            return new ApiResult<string>
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Data = text,
                Error = response.IsSuccessStatusCode ? null : BuildError(text, response.StatusCode)
            };
        }
        catch (HttpRequestException)
        {
            return Failure(HttpStatusCode.ServiceUnavailable,
                "Cannot reach the RaceDay service right now. Please try again shortly.");
        }
        catch (TaskCanceledException)
        {
            return Failure(HttpStatusCode.RequestTimeout,
                "The request took too long. Please try again.");
        }
    }

    private static ApiResult<string> Failure(HttpStatusCode code, string message) =>
        new() { Success = false, StatusCode = code, Error = message };

    // Turns an API error into a friendly message (never a raw error dump).
    private static string BuildError(string body, HttpStatusCode code)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var name in new[] { "message", "detail", "error", "title" })
                {
                    if (doc.RootElement.TryGetProperty(name, out var prop) &&
                        prop.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(prop.GetString()))
                    {
                        return prop.GetString()!;
                    }
                }
            }
        }
        catch (JsonException)
        {
            // The body was not JSON; fall through to the generic messages.
        }

        return code switch
        {
            HttpStatusCode.Unauthorized => "Please log in to continue.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "We could not find what you were looking for.",
            HttpStatusCode.Conflict => "That action conflicts with existing data.",
            HttpStatusCode.BadRequest => "Some of the information entered is not valid.",
            _ => "Something went wrong. Please try again."
        };
    }
}