using Microsoft.AspNetCore.Http;

namespace Application.IDataService;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile imageFile, string? existingImage = null);
    Task DeleteImageAsync(string fileName);
}
