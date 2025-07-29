using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{

    public class FloorplanDevice : BaseModelWithTime, IApplicationEntity
    {
        
        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("type")]
        public DeviceType Type { get; set; }

        [Required]
        [ForeignKey("Floorplan")]
        [Column("floorplan_id")]
        public Guid FloorplanId { get; set; }

        [Required]
        [ForeignKey("AccessCctv")]
        [Column("access_cctv_id")]
        public Guid AccessCctvId { get; set; }

        [Required]
        [ForeignKey("Reader")]
        [Column("ble_reader_id")]
        public Guid ReaderId { get; set; }

        [Required]
        [ForeignKey("AccessControl")]
        [Column("access_control_id")]
        public Guid AccessControlId { get; set; }

        [Required]
        [Column("pos_x")]
        public float PosX { get; set; }

        [Required]
        [Column("pos_y")]
        public float PosY { get; set; }

        [Required]
        [Column("pos_px_x")]
        public float PosPxX { get; set; }

        [Required]
        [Column("pos_px_y")]
        public float PosPxY { get; set; }

        [Required]
        [ForeignKey("FloorplanMaskedArea")]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("device_status")]
        public DeviceStatus DeviceStatus { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstFloorplan Floorplan { get; set; }
        public virtual MstAccessCctv AccessCctv { get; set; }
        public virtual MstBleReader Reader { get; set; }
        public virtual MstAccessControl AccessControl { get; set; }
        public virtual FloorplanMaskedArea FloorplanMaskedArea { get; set; }
        public virtual MstApplication Application { get; set; }
    }
}
