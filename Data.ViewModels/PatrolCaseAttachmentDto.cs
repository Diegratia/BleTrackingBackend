using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class PatrolCaseAttachmentDto : BaseModelDto
    {
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public string? MimeType { get; set; }
        public Guid? PatrolCaseId { get; set; }
        public DateTime? UploadedAt { get; set; }
        public PatrolCaseDto? PatrolCase { get; set; }
    }
    public class PatrolCaseAttachmentCreateDto : BaseModelDto
    {

        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public Guid? PatrolCaseId { get; set; }
    }

    public class PatrolCaseAttachmentUpdateDto : BaseModelDto
    {
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
    }
}