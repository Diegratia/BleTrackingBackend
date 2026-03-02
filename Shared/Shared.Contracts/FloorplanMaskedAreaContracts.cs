using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public class FloorplanMaskedAreaFilter
    {
        public string? Search { get; set; }
        public JsonElement FloorId { get; set; }
        public JsonElement FloorplanId { get; set; }
        public string? RestrictedStatus { get; set; }
        public bool? AllowFloorChange { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? Status { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
    }
}
