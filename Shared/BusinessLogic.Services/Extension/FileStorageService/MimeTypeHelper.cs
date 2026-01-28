using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace BusinessLogic.Services.Extension.FileStorageService
{
    public static class MimeTypeHelper
    {
        private static readonly FileExtensionContentTypeProvider _provider = new();

        public static string GetMimeType(string? fileName)
        {
            if (_provider.TryGetContentType(fileName, out var contentType))
                return contentType;

            return "application/octet-stream";
        }
            public static string GetMimeType(IFormFile file)
        {
            return GetMimeType(file.FileName);
        }
    }

}