using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using System.Diagnostics.CodeAnalysis;

namespace Entities.Models
{
    public class MstTrackingLog
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [MaxLength(50)]
        [Column("beacon_id")]
        public string? BeaconId { get; set; }

        [MaxLength(50)]
        [Column("first_gateway_id")]
        public string? FirstGatewayId { get; set; }

        [MaxLength(50)]
        [Column("second_gateway_id")]
        public string? SecondGatewayId { get; set; }

        [Column("first_distance")]
        public float? FirstDistance { get; set; }

        [Column("second_distance")]
        public float? SecondDistance { get; set; }

        [Column("pos_x")]
        public float? PointX { get; set; }

        [Column("pos_y")]
        public float? PointY { get; set; }

        [Column("is_in_restricted_area")]
        public bool? IsInRestrictedArea { get; set; }

        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        [AllowNull]
        [ForeignKey("FloorplanDevice")]
        [Column("floorplan_device_id")]
        public Guid? FloorplanDeviceId { get; set; }

        [AllowNull]
        [ForeignKey("Floorplan")]
        [Column("floorplan_id")]
        public Guid? FloorplanId { get; set; }

        [AllowNull]
        [ForeignKey("Floor")]
        [Column("floor_id")]
        public Guid? FloorId { get; set; }

        [AllowNull]
        [ForeignKey("FloorplanMaskedArea")]
        [Column("floorplan_masked_area_id")]
        public Guid? FloorplanMaskedAreaId { get; set; }
        public virtual MstFloorplan? Floorplan { get; set; }
        public virtual MstFloor? Floor { get; set; }
        public virtual FloorplanMaskedArea? FloorplanMaskedArea { get; set; }
        public virtual FloorplanDevice? FloorplanDevice { get; set; }
    }
}