using System;
using System.Collections.Generic;

namespace Shared.Contracts.Read
{
    public class MonitoringConfigRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }

        // New many-to-many relationship properties
        public List<Guid> BuildingIds { get; set; } = new();
        public List<string> BuildingNames { get; set; } = new();

        // Old single-building property (kept for backward compatibility)
        [Obsolete("Use BuildingIds instead")]
        public Guid? BuildingId { get; set; }
        [Obsolete("Use BuildingNames instead")]
        public string? BuildingName { get; set; }

        public Guid ApplicationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
