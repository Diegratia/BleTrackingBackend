using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class FloorplanMaskedArea : BaseModel
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("_generate")]
        public int Generate { get; set; }

        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

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

        [Required]
        [StringLength(255)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(255)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstFloor Floor { get; set; }
        public virtual MstFloorplan Floorplan { get; set; }
        public virtual ICollection<VisitorBlacklistArea> BlacklistAreas { get; set; } = new List<VisitorBlacklistArea>();
        public virtual ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}