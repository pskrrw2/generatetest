namespace Application.BlobStorage;
public interface IBlobStorageService
{
    Task DeleteAsync(string fileName);
    Task<string> UploadAsync(byte[] bytes, string fileName);
    Task<string> DisplayImage(string fileName);
    Task<string> DisplayVideoUrl(string fileName);
}