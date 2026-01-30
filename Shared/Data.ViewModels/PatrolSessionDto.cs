using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Shared.Contracts.Read;

namespace Data.ViewModels
{
    
    public class PatrolSessionCreateDto : BaseModelDto
    {
        public Guid? PatrolRouteId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}