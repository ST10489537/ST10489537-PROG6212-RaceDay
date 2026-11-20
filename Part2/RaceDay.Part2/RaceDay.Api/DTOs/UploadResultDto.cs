namespace RaceDay.Api.DTOs
{
    /// <summary>The public address of a file that was stored in Azure Blob Storage.</summary>
    public class UploadResultDto
    {
        /// <summary>Public URL of the uploaded image.</summary>
        public string Url { get; set; } = string.Empty;
    }
}