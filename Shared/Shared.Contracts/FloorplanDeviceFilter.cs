using System;
using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class FloorplanDeviceFilter : BaseFilter
    {
        public JsonElement FloorplanId { get; set; }
        public JsonElement FloorplanMaskedAreaId { get; set; }
        public JsonElement ReaderId { get; set; }
        public JsonElement AccessCctvId { get; set; }
        public JsonElement AccessControlId { get; set; }
        public DeviceType? Type { get; set; }
        public DeviceStatus? DeviceStatus { get; set; }
    }
}
