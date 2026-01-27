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
        public string Config { get; set; }
    }
    public class MonitoringConfigCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }
    }
    public class MonitoringConfigUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }
    }
}