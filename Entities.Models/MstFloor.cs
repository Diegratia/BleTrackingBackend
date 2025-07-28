using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstFloor : BaseModelWithTimeInt ,IApplicationEntity
    {
        [ForeignKey("Building")]
        [Column("building_id")]
        public Guid BuildingId { get; set; }

      
        [Column("name")]
        public string Name { get; set; } 


        [Column("floor_image")]
        public string FloorImage { get; set; }

   
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

    
        [Column("engine_floor_id")]
        public long EngineFloorId { get; set; }

        [Column("status")]
        public int? Status { get; set; } = 1;

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application{ get; set; }
        public virtual MstBuilding Building { get; set; }
        public virtual ICollection<FloorplanMaskedArea> FloorplanMaskedAreas { get; set; } = new List<FloorplanMaskedArea>();
        public virtual ICollection<MstFloorplan> Floorplans { get; set; } = new List<MstFloorplan>();
       
    }
}