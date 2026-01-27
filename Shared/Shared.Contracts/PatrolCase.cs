using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts
{
public class PatrolCaseFilter
    {
        public string? Search { get; set; }

        public CaseType? CaseType { get; set; }
        public CaseStatus? CaseStatus { get; set; }

        public Guid? SecurityId { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        public Guid? PatrolRouteId { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
    }
}