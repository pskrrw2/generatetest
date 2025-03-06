using Application.BlobStorage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Domain.Common.Const;

namespace Infrastructure.BlobStorage;
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadAsync(byte[] bytes, string fileName)
    {
        var client = await GetClientAsync(fileName);

        await using var ms = new MemoryStream(bytes, false);
        await client.UploadAsync(ms, overwrite: true);

        return client.Uri.AbsoluteUri;
    }

    public async Task DeleteAsync(string fileName)
    {
        var client = await GetClientAsync(fileName);

        await client.DeleteAsync();
    }

    public async Task<string> DisplayImage(string fileName)
    {
        string imageUrl = string.Empty;
        try
        {
            string imageBase64 = string.Empty;
            var client = await GetClientAsync(fileName);
            var response = await client.DownloadAsync();

            await using var ms = new MemoryStream();
            await response.Value.Content.CopyToAsync(ms);
            var byteArray = ms.ToArray();
            imageBase64 = Convert.ToBase64String(byteArray);
            return imageUrl = $"data:image/jpeg;base64,{imageBase64}";

        }
        catch (Exception)
        {
            return imageUrl;
        }
    }

    public async Task<string> DisplayVideoUrl(string fileName)
    {
        string videoUrl = string.Empty;
        try
        {
            var client = await GetClientAsync(fileName);
            if (await client.ExistsAsync())
            {
                var sasToken = client.GenerateSasUri(BlobSasPermissions.Read, DateTime.UtcNow.AddHours(1));
                videoUrl = sasToken.AbsoluteUri;
                return videoUrl;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving video: {ex.Message}");
        }

        return videoUrl;
    }

    private async Task<BlobClient> GetClientAsync(string fileName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(Constants.BlobStorageContainerName);

        await container.CreateIfNotExistsAsync();

        return container.GetBlobClient(fileName);
    }
}