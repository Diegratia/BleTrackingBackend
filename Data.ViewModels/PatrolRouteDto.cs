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
        public List<PatrolRouteAreaDto> Areas { get; set; } = new();
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

    }
    public class PatrolRouteUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid?> PatrolAreaIds { get; set; } = new();

    }

    public class PatrolRouteLookUpDto : BaseModelWithAuditDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid?> PatrolAreaIds { get; set; } = new();
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

}