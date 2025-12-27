using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataView;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Extension.FileStorageService
{
   public class FileStorageService : IFileStorageService
{
    private readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/jpg" };

    public async Task<string> SaveImageAsync(IFormFile file, string folder, long maxSize )
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

        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(uploadDir, name);

            try
            {
                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);
            }
            catch (IOException ex)
            {
                throw new BusinessException(
                    "Failed to save image file",
                    ex,
                    "FILE_SAVE_ERROR"
                );
            }

        return $"/Uploads/{folder}/{name}";
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