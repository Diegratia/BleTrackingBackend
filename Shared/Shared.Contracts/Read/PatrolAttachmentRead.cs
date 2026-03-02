using System;

namespace Shared.Contracts.Read
{
    public class PatrolAttachmentRead : BaseRead
    {
        public Guid? PatrolCaseId { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public string? MimeType { get; set; }
        public DateTime? UploadedAt { get; set; }
    }
}
