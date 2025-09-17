using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class AlarmCategorySettings : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("alarm_category")]
        public AlarmRecordStatus? AlarmCategory { get; set; }

        [Column("alarm_level_priority")]
        public AlarmLevelPriority? AlarmLevelPriority { get; set; }

        [Column("alarm_color")]
        public string? AlarmColor { get; set; }
        
        [Column("is_enabled")]
        public int? IsEnabled { get; set; } = 0;

        [Column("application_id")]
        [Required]
        public Guid ApplicationId { get; set; }
    }
}