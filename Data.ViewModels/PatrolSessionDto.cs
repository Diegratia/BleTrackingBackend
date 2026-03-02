using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class PatrolSessionDto : BaseModelDto
    {
        public Guid? PatrolRouteId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public PatrolRouteMinimalDto? PatrolRoute { get; set; }
        public MstSecurityLookUpDto? Security { get; set; }
        public PatrolAssignmentLookUpDto? PatrolAssignment { get; set; }
        
    }
}