using Application.IDataService;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataService
{
    public class ImageService(IWebHostEnvironment webHostEnvironment) : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        public async Task<string> UploadImageAsync(IFormFile imageFile, string? existingImage)
        {

            string uniqueFileName = $"{Guid.NewGuid():N}{Path.GetExtension(imageFile.FileName)}";

            var filePath = Path.Combine("wwwroot/Images", uniqueFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }



            //string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            //if (!Directory.Exists(folderPath))
            //{
            //    Directory.CreateDirectory(folderPath);
            //}

            //string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            //string filePath = Path.Combine(folderPath, uniqueFileName);

            //using (var fileStream = new FileStream(filePath, FileMode.Create))
            //{
            //    await imageFile.CopyToAsync(fileStream);
            //}


            if (!string.IsNullOrEmpty(existingImage))
            {
                await DeleteImageAsync(existingImage);
            }

            return uniqueFileName;
        }

        public async Task DeleteImageAsync(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var filePath = Path.Combine("wwwroot/Images", fileName);
                //  string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        throw; // Rethrow the exception if needed
                    }
                }
            }
        }
    }

}
