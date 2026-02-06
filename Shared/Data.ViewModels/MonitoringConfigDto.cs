using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class MonitoringConfigDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        // New many-to-many relationship property
        public List<Guid> BuildingIds { get; set; } = new();

        // Old single-building property (kept for backward compatibility)
        [Obsolete("Use BuildingIds instead")]
        public Guid? BuildingId { get; set; }

        public string Config { get; set; }
    }
    public class MonitoringConfigCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }

        // New many-to-many relationship property
        public List<Guid> BuildingIds { get; set; } = new();

        // Old single-building property (kept for backward compatibility)
        [Obsolete("Use BuildingIds instead")]
        public Guid? BuildingId { get; set; }
    }
    public class MonitoringConfigUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        // New many-to-many relationship property
        public List<Guid> BuildingIds { get; set; } = new();

        // Old single-building property (kept for backward compatibility)
        [Obsolete("Use BuildingIds instead")]
        public Guid? BuildingId { get; set; }

        public string Config { get; set; }
    }
}