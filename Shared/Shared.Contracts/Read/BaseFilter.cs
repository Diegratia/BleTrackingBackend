using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class BaseFilter
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }

        // Date range filters
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // TimeReport preset (daily, weekly, monthly, etc.) - overrides DateFrom/DateTo
        public string? TimeRange { get; set; }
    }
}
