// File: Entities/Models/TrackingReportPreset.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Entities.Models
{
    [Table("tracking_report_presets")]
    public class TrackingReportPreset : BaseModelWithTime, IApplicationEntity
    {

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("time_range")]
        [StringLength(20)]
        public string TimeRange { get; set; } // "daily", "weekly", "monthly", "custom"

        // Untuk custom preset
        [Column("custom_from_date")]
        public DateTime? CustomFromDate { get; set; }

        [Column("custom_to_date")]
        public DateTime? CustomToDate { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application { get; set; }

        // Constructor
        public TrackingReportPreset()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Status = 1;
        }
    }
}