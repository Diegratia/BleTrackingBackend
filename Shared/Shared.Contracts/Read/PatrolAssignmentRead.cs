using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class PatrolAssignmentRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PatrolDurationType? DurationType { get; set; }
        public PatrolStartType StartType  { get; set; }
        public PatrolCycleType CycleType  { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public Guid? TimeGroupId { get; set; }
        public PatrolApprovalType? ApprovalType { get; set; }
        public PatrolRouteLookUpRead? PatrolRoute { get; set; }
        public AssignmentTimeGroupRead? TimeGroup { get; set; }
        public List<SecurityListRead>? Securities { get; set; } = new();
        public List<PatrolShiftReplacementRead>? ShiftReplacements { get; set; } = new();
        public SecurityListRead? SecurityHead1 { get; set; }
        public SecurityListRead? SecurityHead2 { get; set; }
    }
        public class PatrolAssignmentLookUpRead
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
        }

        public class AssignmentTimeGroupRead
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? ScheduleType { get; set; }
            public List<TimeBlockRead> TimeBlocks { get; set; } = new();
        }
}
