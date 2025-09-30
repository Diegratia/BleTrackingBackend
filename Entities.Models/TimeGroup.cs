using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("time_group")]
    public class TimeGroup : BaseModelWithTime, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("status")]
        public int Status { get; set; }

        // Navigation property

        public MstApplication Application { get; set; }
        public ICollection<TimeBlock> TimeBlocks { get; set; } = new List<TimeBlock>();
        public ICollection<CardAccessTimeGroups?> CardAccessTimeGroups { get; set; } = new List<CardAccessTimeGroups?>();
    }
}
