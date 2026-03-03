using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Contracts.Read;

namespace Shared.Contracts
{

    public class PatrolSessionFilter : BaseFilter
    {


        // 🔗 Foreign Keys
        public Guid? PatrolRouteId { get; set; }

        public Guid? SecurityId { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        
        public DateTime? EndedAt { get; set; }
    }
}
