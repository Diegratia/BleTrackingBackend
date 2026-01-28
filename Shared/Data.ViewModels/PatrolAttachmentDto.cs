using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;

namespace Data.ViewModels
{
    public class PatrolAttachmentDto : BaseModelWithAuditDto
    {
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public string? MimeType { get; set; }
        public DateTime? UploadedAt { get; set; }
        public Guid? PatrolCaseId { get; set; }
    }
    public class PatrolAttachmentCreateDto : BaseModelDto
    {
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public Guid? PatrolCaseId { get; set; }
    }
    public class PatrolAttachmentUpdateDto
    {
        // public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
    }
}