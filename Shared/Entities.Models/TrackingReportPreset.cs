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

        [Column("building_id")]
        public Guid? BuildingId { get; set; }
        [Column("floor_id")]
        public Guid? FloorId { get; set; }

        [Column("floorplan_id")]
        public Guid? FloorplanId { get; set; }

        [Column("area_id")]
        public Guid? AreaId { get; set; }
        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }
        [Column("member_id")]
        public Guid? MemberId { get; set; }

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
        public bool IsCustomRange =>
        string.Equals(TimeRange, "custom", StringComparison.OrdinalIgnoreCase);
    }
}