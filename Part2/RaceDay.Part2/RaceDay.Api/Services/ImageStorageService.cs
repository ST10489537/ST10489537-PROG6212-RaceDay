using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    /// <summary>Stores uploaded images and returns their public URL.</summary>
    public interface IImageStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);
    }

    /// <summary>Stores images in Azure Blob Storage.</summary>
    public class BlobImageStorageService : IImageStorageService
    {
        private readonly IConfiguration _config;

        public BlobImageStorageService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            // Read lazily so the app (and the unit tests) still start when
            // no connection string is configured.
            var connectionString = _config["AzureBlob:ConnectionString"];
            var containerName = _config["AzureBlob:ContainerName"] ?? "raceday-images";

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Image storage is not configured.");

            var container = new BlobContainerClient(connectionString, containerName);
            await container.CreateIfNotExistsAsync();

            // A random name stops two uploads overwriting each other.
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blob.Uri.ToString();
        }
    }
}