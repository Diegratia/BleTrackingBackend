using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataView;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace BusinessLogic.Services.Extension.FileStorageService
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string[] AllowedTypes =
        {
            "image/jpeg",
            "image/png",
            "image/jpg"
        };

       public async Task<string> SaveImageAsync(
        IFormFile file,
        string folder,
        long maxSize,
        ImagePurpose purpose,
        int maxWidth = 1280,
        int quality = 75
    )
    {
        if (!AllowedTypes.Contains(file.ContentType))
            throw new BusinessException("Invalid image type");

        if (file.Length > maxSize)
            throw new BusinessException(
                $"File size exceeds {maxSize / 1024 / 1024} MB limit"
            );

        var basePath = AppContext.BaseDirectory;
        var uploadDir = Path.Combine(basePath, "Uploads", folder);
        Directory.CreateDirectory(uploadDir);

        var ext = Path.GetExtension(file.FileName).ToLower();

        // 🔒 Floorplan TIDAK BOLEH JPEG
        if (purpose == ImagePurpose.Floorplan && ext is ".jpg" or ".jpeg")
            throw new BusinessException("Floorplan must be PNG (lossless)");

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadDir, fileName);

        try
        {
            using var image = await Image.LoadAsync(file.OpenReadStream());
            image.Mutate(x => x.AutoOrient());

            if (image.Width > maxWidth)
            {
                image.Mutate(x =>
                    x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxWidth, 0)
                    })
                );
            }

            if (purpose == ImagePurpose.Floorplan)
            {
                // 🗺️ LOSSLESS
                await image.SaveAsync(fullPath, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.Level9
                });
            }
            else
            {
                // 📸 PHOTO (LOSSY)
                await image.SaveAsync(fullPath, new JpegEncoder
                {
                    Quality = quality
                });
            }
        }
        catch (Exception ex)
        {
            throw new BusinessException(
                "Failed to process image",
                ex,
                "IMAGE_PROCESS_ERROR"
            );
        }

        return $"/Uploads/{folder}/{fileName}";
    }


        public Task DeleteAsync(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            var fullPath = Path.Combine(
                AppContext.BaseDirectory,
                relativePath.TrimStart('/')
            );

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException ex)
                {
                    throw new BusinessException(
                        "Failed to delete image file",
                        ex,
                        "FILE_DELETE_ERROR"
                    );
                }
            }

            return Task.CompletedTask;
        }
    }
}
