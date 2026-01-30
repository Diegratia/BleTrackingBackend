using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class PatrolAssignmentCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public Guid? TimeGroupId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid> SecurityIds { get; set; } = new();
    }

    public class PatrolAssignmentUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public Guid? TimeGroupId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid?> SecurityIds { get; set; } = new();
    }


}