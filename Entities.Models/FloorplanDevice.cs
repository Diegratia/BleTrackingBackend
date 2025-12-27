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
        
        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [Column("type")]
        public DeviceType? Type { get; set; }

        [Required]
        [Column("floorplan_id")]
        public Guid FloorplanId { get; set; }

        [Column("access_cctv_id")]
        public Guid? AccessCctvId { get; set; }

        [Column("ble_reader_id")]
        public Guid? ReaderId { get; set; }

        [Column("access_control_id")]
        public Guid? AccessControlId { get; set; }

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

        [Column("path")]
        public string? Path { get; set; }

        [Required]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("device_status")]
        public DeviceStatus? DeviceStatus { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; } = 1;

        public MstFloorplan Floorplan { get; set; }
        public MstAccessCctv? AccessCctv { get; set; }
        public MstBleReader? Reader { get; set; }
        public MstAccessControl? AccessControl { get; set; }
        public FloorplanMaskedArea FloorplanMaskedArea { get; set; }
        public MstApplication Application { get; set; }
    }
}
