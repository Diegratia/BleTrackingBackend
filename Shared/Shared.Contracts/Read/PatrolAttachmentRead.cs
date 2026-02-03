using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class PatrolAttachmentRead : BaseRead
    {
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public string? MimeType { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}