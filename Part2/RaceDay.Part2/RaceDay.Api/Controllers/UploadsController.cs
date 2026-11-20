using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    /// <summary>Uploads images to Azure Blob Storage and returns their public URL.</summary>
    [ApiController]
    [Route("api/uploads")]
    [SessionAuthorize]   // every action needs a logged-in user; the banner upload also needs the Organiser role
    public class UploadsController : ControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        private readonly IImageStorageService _storage;

        public UploadsController(IImageStorageService storage)
        {
            _storage = storage;
        }

        /// <summary>Uploads an event banner image. Organisers only.</summary>
        /// <param name="request">Form data containing a JPG, PNG or WEBP image of up to 5 MB.</param>
        /// <response code="200">The public URL of the stored image.</response>
        /// <response code="400">No file, wrong file type, or file too large.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser.</response>
        [HttpPost("event-banner")]
        [SessionAuthorize("Organiser")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public Task<IActionResult> UploadEventBanner([FromForm] ImageUploadRequest request) =>
            SaveAsync(request.File, "event-banners");

        /// <summary>Uploads a profile picture for the logged-in user. Available to both roles.</summary>
        /// <param name="request">Form data containing a JPG, PNG or WEBP image of up to 5 MB.</param>
        /// <response code="200">The public URL of the stored image.</response>
        /// <response code="400">No file, wrong file type, or file too large.</response>
        /// <response code="401">Not logged in.</response>
        [HttpPost("profile-picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> UploadProfilePicture([FromForm] ImageUploadRequest request) =>
            SaveAsync(request.File, "profile-pictures");

        private async Task<IActionResult> SaveAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Please choose an image to upload." });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new { message = "Only JPG, PNG and WEBP images are allowed." });

            if (file.Length > MaxBytes)
                return BadRequest(new { message = "The image must be 5 MB or smaller." });

            try
            {
                var url = await _storage.UploadAsync(file, folder);
                return Ok(new UploadResultDto { Url = url });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
            }
            catch (Azure.RequestFailedException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "We could not store the image. Please try again." });
            }
        }
    }
}