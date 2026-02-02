using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class PatrolSessionRead : BaseRead
    {
        public Guid? PatrolRouteId { get; set; }
        public string? PatrolRouteNameSnap { get; set; }
        public Guid? SecurityId { get; set; }
        public string? SecurityNameSnap { get; set; }
        public string? SecurityIdentityIdSnap{ get; set; }
        public string? SecurityCardNumberSnap { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        public string? PatrolAssignmentNameSnap { get; set; }
        public Guid? TimeGroupId { get; set; }
        public string? TimeGroupNameSnap { get; set; }
        public string? StartAreaNameSnap{ get; set; }
        public string? EndAreaNameSnap { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        // Checkpoint Summary & Details
        public int CheckpointCount { get; set; }
        public List<PatrolCheckpointLogRead> Checkpoints { get; set; } = new();
        
        // public PatrolRouteLookUpRead? PatrolRoute { get; set; }
        // public MstSecurityLookUpRead? Security { get; set; }
        // public PatrolAssignmentLookUpRead? PatrolAssignment { get; set; }
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