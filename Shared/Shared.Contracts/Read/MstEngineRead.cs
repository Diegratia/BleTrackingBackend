using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class MstEngineRead : BaseRead
    {
        public string? Name { get; set; }
        public string? EngineTrackingId { get; set; }
        public int? Port { get; set; }
        public int? IsLive { get; set; }
        public DateTime? LastLive { get; set; }
        public string? ServiceStatus { get; set; }
    }
}
