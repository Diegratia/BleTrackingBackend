using System;
using System.Text.Json;

namespace Shared.Contracts
{
    public class FloorplanDeviceFilter
    {
        public string? Search { get; set; }
        public JsonElement FloorplanId { get; set; }
        public JsonElement FloorplanMaskedAreaId { get; set; }
        public JsonElement ReaderId { get; set; }
        public JsonElement AccessCctvId { get; set; }
        public JsonElement AccessControlId { get; set; }
        public DeviceType? Type { get; set; }
        public DeviceStatus? DeviceStatus { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? Status { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
    }
}
