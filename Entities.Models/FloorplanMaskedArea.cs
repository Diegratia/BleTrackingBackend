using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class FloorplanMaskedArea : BaseModelWithTime, IApplicationEntity
    {

        [Required]
        [ForeignKey("Floorplan")]
        [Column("floorplan_id")]
        public Guid FloorplanId { get; set; }

        [Required]
        [ForeignKey("Floor")]
        [Column("floor_id")]
        public Guid FloorId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("area_shape")]
        public string AreaShape { get; set; }

        [Required]
        [StringLength(255)]
        [Column("color_area")]
        public string ColorArea { get; set; }

        [Required]
        [Column("restricted_status")]
        public RestrictedStatus RestrictedStatus { get; set; }

        [Required]
        [StringLength(255)]
        [Column("engine_area_id")]
        public string EngineAreaId { get; set; }

        // [Required]
        // [Column("wide_area")]
        // public long WideArea { get; set; }

        // [Required]
        // [Column("position_px_x")]
        // public long PositionPxX { get; set; }

        // [Required]
        // [Column("position_px_y")]
        // public long PositionPxY { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual MstFloor Floor { get; set; }
        public virtual MstFloorplan Floorplan { get; set; }
        public virtual ICollection<VisitorBlacklistArea> BlacklistAreas { get; set; } = new List<VisitorBlacklistArea>();
        public virtual ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}