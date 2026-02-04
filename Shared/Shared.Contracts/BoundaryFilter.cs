using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public class BoundaryFilter
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }

        // Entity-specific filters
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? Status { get; set; }
        public int? IsActive { get; set; }
        public BoundaryType? BoundaryType { get; set; }

        // Use JsonElement to support both single Guid and Guid array
        public JsonElement FloorplanId { get; set; }
        public JsonElement FloorId { get; set; }
    }
}
