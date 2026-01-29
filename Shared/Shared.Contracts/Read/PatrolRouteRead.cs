using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class PatrolRouteLookUpRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? StartAreaName { get; set; }
        public string? EndAreaName { get; set; }
    }
        
    
}