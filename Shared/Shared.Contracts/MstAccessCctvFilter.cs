using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public class MstAccessCctvFilter
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
        public bool? IsAssigned { get; set; }

        // Use JsonElement to support both single Guid and Guid array
        public JsonElement IntegrationId { get; set; }
    }
}
