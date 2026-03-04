using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Data.ViewModels
{
    
    public class PatrolSessionStartDto : BaseModelDto
    {
        public Guid? PatrolAssignmentId { get; set; }
    }

    public class PatrolCheckpointActionDto
    {
        public Guid PatrolCheckpointLogId { get; set; }
        public Guid PatrolAreaId { get; set; }
        public PatrolCheckpointStatus PatrolCheckpointStatus { get; set; } 
        public string? SecurityNote { get; set; }
        public PatrolCaseCreateDto? CaseDetails { get; set; }

    }
}