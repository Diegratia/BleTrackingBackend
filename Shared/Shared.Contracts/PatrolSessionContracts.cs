using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts
{

    public class PatrolSessionFilter
    {
        // 🔎 Global search (Name / Description)
        public string? Search { get; set; }

        // 🔗 Foreign Keys
        public Guid? PatrolRouteId { get; set; }

        public Guid? SecurityId { get; set; }
        public Guid? PatrolAssignmentId { get; set; }
        

        // 📅 Date range
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // 📄 Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // ↕ Sorting
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
    }
}
