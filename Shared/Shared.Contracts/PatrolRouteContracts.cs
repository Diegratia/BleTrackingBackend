namespace Shared.Contracts
{
    public class PatrolRouteFilter
    {
        // 🔎 Search Name atau Description
        public string? Search { get; set; }

        // 📅 Date range berdasarkan UpdatedAt
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