using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstFloorplan : BaseModelWithTime, IApplicationEntity
    {
        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [Column("floorplan_image")]
        public string? FloorplanImage { get; set; }

        [Column("pixel_x")]
        public float PixelX { get; set; }


        [Column("pixel_y")]
        public float PixelY { get; set; }


        [Column("floor_x")]
        public float FloorX { get; set; }


        [Column("floor_y")]
        public float FloorY { get; set; }


        [Column("meter_per_px")]
        public float MeterPerPx { get; set; }

        [Column("engine_id")]
        public Guid? EngineId { get; set; }

        [Required]
        [Column("floor_id")]
        public Guid FloorId { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public MstApplication Application { get; set; }
        public MstFloor Floor { get; set; }
        public MstEngine? Engine { get; set; }
        public ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
        public ICollection<FloorplanMaskedArea> FloorplanMaskedAreas { get; set; } = new List<FloorplanMaskedArea>();
    }
}










// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;
// using Helpers.Consumer;

// namespace Entities.Models
// {
//     public class MstFloorplan : BaseModelWithTime, IApplicationEntity
//     {
//         [StringLength(255)]
//         [Column("name")]
//         public string? Name { get; set; }

//         [Column("floorplan_image")]
//         public string? FloorplanImage { get; set; }

//          [Column("pixel_x")]
//         public float PixelX { get; set; }


//         [Column("pixel_y")]
//         public float PixelY { get; set; }


//         [Column("floor_x")]
//         public float FloorX { get; set; }


//         [Column("floor_y")]
//         public float FloorY { get; set; }


//         [Column("meter_per_px")]
//         public float MeterPerPx { get; set; }

//         [Column("engine_id")]
//         public Guid? EngineId { get; set; }

//         [Required]
//         [Column("floor_id")]
//         public Guid FloorId { get; set; }

//         [Required]
//         [ForeignKey("Application")]
//         [Column("application_id")]
//         public Guid ApplicationId { get; set; }

//         [Required]
//         [Column("status")]
//         public int? Status { get; set; } = 1;

//         public  MstApplication Application { get; set; }
//         public MstFloor Floor { get; set; }
//         public MstEngine Engine { get; set; }
//         public ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
//         public ICollection<FloorplanMaskedArea> FloorplanMaskedAreas { get; set; } = new List<FloorplanMaskedArea>();
//     }
// }

