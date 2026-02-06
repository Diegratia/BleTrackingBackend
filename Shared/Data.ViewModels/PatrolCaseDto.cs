using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Helpers.Consumer;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Data.ViewModels
{
    public class PatrolCaseCreateDto : BaseModelDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public CaseType? CaseType { get; set; }
        public Guid? PatrolSessionId { get; set; }
        public List<PatrolAttachmentCreateDto>? Attachments { get; set; }
    }

    public class PatrolCaseUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public CaseType? CaseType { get; set; }
        /// <summary>
        /// Attachments to APPEND (not replace). Use DeleteAttachment endpoint to remove.
        /// </summary>
        public List<PatrolAttachmentCreateDto>? Attachments { get; set; }
    }

    /// <summary>
    /// DTO for approve/reject operations
    /// </summary>
    public class PatrolCaseApprovalDto
    {
        public string? Reason { get; set; }  // Optional for approve, Required for reject
    }

    /// <summary>
    /// DTO for close operation
    /// </summary>
    public class PatrolCaseCloseDto
    {
        public string? Notes { get; set; }  // Optional closing notes
    }

}