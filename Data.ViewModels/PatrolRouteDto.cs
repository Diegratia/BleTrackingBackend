using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class PatrolRouteDto : BaseModelWithAuditDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? PatrolAreaCount { get; set; }
        public List<PatrolRouteAreaDto> PatrolAreas { get; set; } = new();
        public List<PatrolTimeGroupDto> PatrolTimeGroups { get; set; } = new();
        // public List<Guid?> TimeGroupIds { get; set; } = new();
        // public List<Guid?> PatrolAreaIds { get; set; } = new();
        // public float EstimatedDistance { get; set; }
        // public int EstimatedTime  { get; set; }
        // public Guid? StartAreaId { get; set; }
        // public Guid? EndAreaId { get; set; }
    }
    public class PatrolRouteCreateDto : BaseModelDto 
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid?> PatrolAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
    }
    public class PatrolRouteUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid?> PatrolAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
    }

    public class PatrolRouteLookUpDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid?> PatrolAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
    }

            public class PatrolRouteAreaDto
        {
            public Guid PatrolAreaId { get; set; }
            public int OrderIndex { get; set; }

            public float EstimatedDistance { get; set; }
            public int EstimatedTime { get; set; }

            public Guid? StartAreaId { get; set; }
            public Guid? EndAreaId { get; set; }
        }

            public class PatrolTimeGroupDto
        {
            public Guid TimeGroupId { get; set; }

            public string? Name { get; set; }
            public string? ScheduleType { get; set; }
        }
}