using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class AlarmCategorySettingsDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? AlarmCategory { get; set; }
        public string? Remarks { get; set; }
        public string? AlarmColor { get; set; }
        public string? AlarmLevelPriority { get; set; }
        public int? NotifyIntervalSec { get; set; } 
        public int? IsEnabled { get; set; }
    }
    // public class AlarmCategorySettingsCreateDto : BaseModelDto
    // {
    //     public string? AlarmColor { get; set; }
    //     public string? AlarmCategory { get; set; }
    //     public string? Remarks { get; set; }
    //     public string? AlarmLevelPriority { get; set; }
    //     public int? NotifyIntervalSec { get; set; } 
    //     public int? IsEnabled { get; set; } = 0;
    // }
    public class AlarmCategorySettingsUpdateDto
    {
        public string? AlarmColor { get; set; }
        public string? Remarks { get; set; }
        public string? AlarmLevelPriority { get; set; }
        public int? NotifyIntervalSec { get; set; } 
        public int? IsEnabled { get; set; }
    }
}