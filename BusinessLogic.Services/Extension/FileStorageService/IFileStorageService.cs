using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Extension.FileStorageService
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(
            IFormFile file,
            string folder,
            long maxSize,
            ImagePurpose purpose,
            int maxWidth = 1280,
            int quality = 75
        );
        Task DeleteAsync(string relativePath);
    }

}