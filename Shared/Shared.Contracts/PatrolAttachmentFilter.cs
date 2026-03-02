using System;

namespace Shared.Contracts
{
    public class PatrolAttachmentFilter : Shared.Contracts.Read.BaseFilter
    {
        public Guid? PatrolCaseId { get; set; }
        public FileType? FileType { get; set; }
    }
}
