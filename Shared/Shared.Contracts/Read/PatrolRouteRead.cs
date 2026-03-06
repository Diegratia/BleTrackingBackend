using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class PatrolRouteRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StartAreaName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EndAreaName { get; set; }
        public int PatrolAreaCount { get; set; }
        
        // List detail area dalam route
        public List<PatrolRouteAreaReadDto> PatrolAreas { get; set; } = new();
        
    }

        public class PatrolRouteAreaReadDto
    {
            public Guid PatrolAreaId { get; set; }
            public int OrderIndex { get; set; }
            public float? EstimatedDistance { get; set; }
            public double? EstimatedTime { get; set; }
            public int? MinDwellTime { get; set; }
            public int? MaxDwellTime { get; set; }
            public Guid? StartAreaId { get; set; }
            public Guid? EndAreaId { get; set; }
    }

    public class PatrolRouteLookUpRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StartAreaName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EndAreaName { get; set; }
    }
        
    
}