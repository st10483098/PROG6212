using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobStorageService
{
    private readonly BlobContainerClient _container;

    public BlobStorageService(IConfiguration config)
    {
        var conn = config["BlobStorage:ConnectionString"]
                   ?? throw new InvalidOperationException("BlobStorage:ConnectionString is not configured.");
        var name = config["BlobStorage:Container"] ?? "raceday-images";
        _container = new BlobContainerClient(conn, name);
        _container.CreateIfNotExists(PublicAccessType.Blob); // images are publicly readable by URL
    }

    /// <summary>Uploads the file and returns its public URL.</summary>
    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blob = _container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{ext}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        return blob.Uri.ToString();
    }

    public async Task DeleteByUrlAsync(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        var blobName = new Uri(url).AbsolutePath.TrimStart('/').Substring(_container.Name.Length + 1);
        await _container.DeleteBlobIfExistsAsync(blobName);
    }
}
