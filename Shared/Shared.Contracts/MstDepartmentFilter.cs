using System;

namespace Shared.Contracts
{
    public class MstDepartmentFilter
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
        
        public int? Status { get; set; }
    }
}
