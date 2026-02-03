using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public Guid? ApprovedByHeadId { get; set; }
        public Guid? PatrolAssignmentId { get; set; } // snapshot dari assignment
        public Guid? PatrolRouteId { get; set; }    // snapshot dari assignment
        public MstSecurityLookUpRead? Security { get; set; }
        public PatrolAssignmentLookUpRead? PatrolAssignment { get; set; }
        public PatrolRouteLookUpRead? PatrolRoute { get; set; }
        public List<PatrolAttachmentRead>? Attachments { get; set; }
    }


}