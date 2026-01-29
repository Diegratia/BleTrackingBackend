using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class PatrolAssignmentRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public Guid? TimeGroupId { get; set; }
        public PatrolRouteLookUpRead? PatrolRoute { get; set; }
        public AssignmentTimeGroupRead? TimeGroup { get; set; }
        public List<SecurityListRead>? Securities { get; set; } = new();

    }
    
    public class PatrolRouteLookUpRead
        {
            public Guid Id { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? StartAreaName { get; set; }
            public string? EndAreaName { get; set; }
        }

        public class PatrolAssignmentLookUpRead
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
        }

        public class SecurityListRead
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? CardNumber { get; set; }
            public string? IdentityId { get; set; }
            public string? OrganizationName { get; set; }
            public string? DepartmentName { get; set; }
            public string? DistrictName { get; set; }
        }

        public class AssignmentTimeGroupRead
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? ScheduleType { get; set; }
            public List<TimeBlockRead> TimeBlocks { get; set; } = new();
        }

        public class TimeBlockRead
        {
            public Guid Id { get; set;}
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public DayOfWeek? DayOfWeek { get; set; }  
            public TimeSpan? StartTime { get; set; }
            public TimeSpan? EndTime { get; set; }
        }
}
