using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class PatrolRouteAreaRequestDto
    {
        public Guid PatrolAreaId { get; set; }
        public int? MinDwellTime { get; set; }
        public int? MaxDwellTime { get; set; }
    }

    public class PatrolRouteCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<PatrolRouteAreaRequestDto> RouteAreas { get; set; } = new();
    }
    public class PatrolRouteUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<PatrolRouteAreaRequestDto> RouteAreas { get; set; } = new();

    }     
}