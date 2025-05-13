using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class FloorplanMaskedArea
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Generate { get; set; }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Floorplan")]
        public Guid FloorplanId { get; set; }

        [Required]
        [ForeignKey("Floor")]
        public Guid FloorId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string AreaShape { get; set; }

        [Required]
        [StringLength(255)]
        public string ColorArea { get; set; }

        [Required]
        public RestrictedStatus RestrictedStatus { get; set; }

        [Required]
        [StringLength(255)]
        public string EngineAreaId { get; set; }

        [Required]
        public long WideArea { get; set; }

        [Required]
        public long PositionPxX { get; set; }

        [Required]
        public long PositionPxY { get; set; }

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public int? Status { get; set; } = 1;


        public virtual AlarmRecordTracking AlarmRecordTracking { get; set; }
        public virtual MstFloor Floor { get; set; }
        public virtual MstFloorplan Floorplan { get; set; }
        public virtual ICollection<VisitorBlacklistArea> BlacklistAreas { get; set; } = new List<VisitorBlacklistArea>();
        public virtual ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}