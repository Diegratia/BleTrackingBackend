using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class PatrolSessionRead : BaseRead
    {
        public Guid? PatrolRouteId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public PatrolRouteLookUpRead? PatrolRoute { get; set; }
        public MstSecurityLookUpRead? Security { get; set; }
        public PatrolAssignmentLookUpRead? PatrolAssignment { get; set; }
    }
    public class PatrolSessionLookUpRead : BaseRead
    {
        public Guid? PatrolRouteId { get; set; }
        public string? PatrolRouteName { get; set; }
        public Guid? SecurityId { get; set; }
        public string? SecurityName { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        public string? PatrolAssignmentName { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}