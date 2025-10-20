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
        [Column("floorplan_id")]
        public Guid FloorplanId { get; set; }

        [Required]
        [Column("floor_id")]
        public Guid FloorId { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [Required]
        [Column("area_shape")]
        public string AreaShape { get; set; }   

        [StringLength(255)]
        [Column("color_area")]
        public string? ColorArea { get; set; }

        [Required]
        [Column("restricted_status")]
        public RestrictedStatus RestrictedStatus { get; set; }

        // [Required]
        // [StringLength(255)]
        // [Column("engine_area_id")]
        // public string EngineAreaId { get; set; }

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
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public MstApplication Application { get; set; }
        public MstFloor Floor { get; set; }
        public MstFloorplan Floorplan { get; set; }
        public ICollection<BlacklistArea> BlacklistAreas { get; set; } = new List<BlacklistArea>();
        public ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
        public ICollection<CardAccessMaskedArea> CardAccessMaskedAreas { get; set; } = new List<CardAccessMaskedArea>();
    }
}