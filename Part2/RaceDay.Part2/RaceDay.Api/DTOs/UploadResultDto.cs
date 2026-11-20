namespace RaceDay.Api.DTOs
{
    /// <summary>The public address of a file that was stored in Azure Blob Storage.</summary>
    public class UploadResultDto
    {
        /// <summary>Public URL of the uploaded image.</summary>
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>The form data sent when uploading an image.</summary>
    public class ImageUploadRequest
    {
        /// <summary>The image file: a JPG, PNG or WEBP of up to 5 MB.</summary>
        public IFormFile? File { get; set; }
    }
}