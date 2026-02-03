using System;

namespace Shared.Contracts
{
    public class MstDistrictFilter
    {
        // 🔎 Standard DataTables
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }

        // 📅 Date Filters
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        
        // 🏷 Specific Filters
        public int? Status { get; set; }
    }
}
