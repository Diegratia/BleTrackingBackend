using System;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("time_blocks")]
    public class TimeBlock : BaseModelWithTime, IApplicationEntity
    {
        
        [Column("day_of_week")]
        public DayOfWeek? DayOfWeek { get; set; } // 0=Sunday, 1=Monday, ...

        [Column("start_time")]
        public TimeSpan? StartTime { get; set; }

        [Column("end_time")]
        public TimeSpan? EndTime { get; set; }

        [ForeignKey("TimeGroup")]
        [Column("time_group_id")]
        public Guid? TimeGroupId { get; set; }

        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("status")]
        public int Status { get; set; }

        // Navigation property

        public MstApplication Application { get; set; }
        public TimeGroup TimeGroup { get; set; }
    }
}
