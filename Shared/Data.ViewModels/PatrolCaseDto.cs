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
    public class PatrolCaseDto : BaseModelWithAuditDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public CaseType? CaseType { get; set; }
        public CaseStatus? CaseStatus { get; set; }
        public Guid? PatrolSessionId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? ApprovedByHeadId { get; set; }
        public Guid? PatrolAssignmentId { get; set; } // snapshot dari assignment
        public Guid? PatrolRouteId { get; set; }    // snapshot dari assignment
        public MstSecurityLookUpDto? Security { get; set; }
        public PatrolAssignmentLookUpRead? PatrolAssignment { get; set; }
        public PatrolRouteMinimalDto? PatrolRoute { get; set; }
    }
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
        public List<PatrolAttachmentUpdateDto>? Attachments { get; set; }
    }

    public class PatrolCaseCreateManualDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid PatrolAssignmentId { get; set; }
        public Guid SecurityId { get; set; }
    }



}