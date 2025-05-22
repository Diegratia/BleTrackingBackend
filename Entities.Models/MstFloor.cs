using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstFloor
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("generate")]
        public int Generate { get; set; } 

        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid(); 

      
        [ForeignKey("Building")]
        [Column("building_id")]
        public Guid BuildingId { get; set; }

      
        [Column("name")]
        public string Name { get; set; } 


        [Column("floor_image")]
        public string FloorImage { get; set; }

   
        [Column("pixel_x")]
        public long PixelX { get; set; }

    
        [Column("pixel_y")]
        public long PixelY { get; set; }


        [Column("floor_x")]
        public long FloorX { get; set; }


        [Column("floor_y")]
        public long FloorY { get; set; }


        [Column("meter_per_px")]
        public decimal MeterPerPx { get; set; }

    
        [Column("engine_floor_id")]
        public long EngineFloorId { get; set; }

   
        [StringLength(255)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

    
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

     
        [StringLength(255)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; }

     
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstBuilding Building { get; set; }
        public virtual ICollection<FloorplanMaskedArea> FloorplanMaskedAreas { get; set; } = new List<FloorplanMaskedArea>();
        public virtual ICollection<MstFloorplan> Floorplans { get; set; } = new List<MstFloorplan>();
       
    }
}