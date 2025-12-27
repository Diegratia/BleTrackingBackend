using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Extension.FileStorageService
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file, string folder, long maxSize);
        Task DeleteAsync(string relativePath);
    }

}