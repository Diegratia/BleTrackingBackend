using System;

namespace Shared.Contracts
{
    public class MstOrganizationFilter
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
        
        // Specific filters can be added here
        public int? Status { get; set; }
    }
}
