using System;
using System.Collections.Generic;

namespace Shared.Contracts.Read
{
    public class MonitoringConfigRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }
        public List<Guid> BuildingIds { get; set; } = new();
        public List<string> BuildingNames { get; set; } = new();
    }
}
