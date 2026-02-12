using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class PatrolCaseRead : BaseRead
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public CaseType? CaseType { get; set; }
        public CaseStatus? CaseStatus { get; set; }
        public Guid? PatrolSessionId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? SecurityHead1Id { get; set; }
        public Guid? SecurityHead2Id { get; set; }
        public PatrolApprovalType? ApprovalType { get; set; }
        public Guid? ApprovedByHead1Id { get; set; }
        public string? ApprovedByHead1Name { get; set; }
        public string? ApprovedByHead1Identity { get; set; }
        public Guid? ApprovedByHead2Id { get; set; }
        public string? ApprovedByHead2Name { get; set; }
        public string? ApprovedByHead2Identity { get; set; }
        public DateTime? ApprovedByHead1At { get; set; }
        public DateTime? ApprovedByHead2At { get; set; }
        public Guid? PatrolAssignmentId { get; set; } // snapshot dari assignment
        public Guid? PatrolRouteId { get; set; }    // snapshot dari assignment
        public MstSecurityLookUpRead? Security { get; set; }
        public PatrolAssignmentLookUpRead? PatrolAssignment { get; set; }
        public PatrolRouteLookUpRead? PatrolRoute { get; set; }
        public List<PatrolAttachmentRead>? Attachments { get; set; }
    }


}
