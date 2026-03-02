using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class MonitoringConfigCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }
        public List<Guid> BuildingIds { get; set; } = new();
    }

    public class MonitoringConfigUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid> BuildingIds { get; set; } = new();
        public string Config { get; set; }
    }
}