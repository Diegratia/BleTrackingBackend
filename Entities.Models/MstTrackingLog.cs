using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstTrackingLog
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("beacon_id")]
        public string BeaconId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("pair")]
        public string Pair { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("first_reader_id")]
        public string FirstReaderId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("second_reader_id")]
        public string SecondReaderId { get; set; }

        // [Required]
        // [Column("first_dist")]
        // public decimal FirstDist { get; set; }

        // [Required]
        // [Column("second_dist")]
        // public decimal SecondDist { get; set; }

        // [Required]
        // [Column("jarak_meter")]
        // public decimal JarakMeter { get; set; }

        [Required]
        [Column("point_x")]
        public decimal PointX { get; set; }

        [Required]
        [Column("point_y")]
        public decimal PointY { get; set; }

        [Required]
        [Column("first_reader_x")]
        public decimal FirstReaderX { get; set; }

        [Required]
        [Column("first_reader_y")]
        public decimal FirstReaderY { get; set; }

        [Required]
        [Column("second_reader_x")]
        public decimal SecondReaderX { get; set; }

        [Required]
        [Column("second_reader_y")]
        public decimal SecondReaderY { get; set; }

        [Required]
        [Column("time")]
        public DateTime Time { get; set; }

        [Required]
        [ForeignKey("FloorplanDevice")]
        [Column("floorplan_id")]
        public Guid FloorplanId { get; set; }

        public virtual FloorplanDevice FloorplanDevices { get; set; }
    }
}