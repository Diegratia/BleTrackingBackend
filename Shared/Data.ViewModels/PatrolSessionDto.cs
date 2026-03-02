using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Shared.Contracts.Read;

namespace Data.ViewModels
{
    
    public class PatrolSessionStartDto : BaseModelDto
    {
        public Guid? PatrolAssignmentId { get; set; }
    }
}