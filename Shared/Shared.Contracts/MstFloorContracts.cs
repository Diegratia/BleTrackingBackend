using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public class MstFloorFilter
    {
        public string? Search { get; set; }
        public JsonElement BuildingId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? Status { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
    }
}
