using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class TrackingTransaction : IApplicationEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("trans_time")]
        public DateTime TransTime { get; set; }

        [Required]
        [ForeignKey("Reader")]
        [Column("reader_id")]
        public Guid ReaderId { get; set; } 

        [Required]
        [Column("card_id")]
        public long CardId { get; set; }

        [Required]
        [ForeignKey("FloorplanMaskedArea")]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Required]
        [Column("coordinate_x")]
        public float CoordinateX { get; set; }

        [Required]
        [Column("coordinate_y")]
        public float CoordinateY { get; set; }

        [Required]
        [Column("coordinate_px_x")]
        public float CoordinatePxX { get; set; }

        [Required]
        [Column("coordinate_px_y")]
        public float CoordinatePxY { get; set; }

        [Required]
        [Column("alarm_status")]
        public AlarmStatus AlarmStatus { get; set; }

        [Required]
        [Column("battery")]
        public long Battery { get; set; }

        [Required]
        [ForeignKey("ApplicationId")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual MstBleReader Reader { get; set; }

        public virtual FloorplanMaskedArea FloorplanMaskedArea { get; set; }
    }
}