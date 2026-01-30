using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
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
}