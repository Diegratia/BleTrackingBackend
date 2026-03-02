using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("evacuation_assembly_points")]
    public class EvacuationAssemblyPoint : BaseModelWithTime, IApplicationEntity
    {
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("area_shape")]
        public string? AreaShape { get; set; }

        [Column("color")]
        public string? Color { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [ForeignKey(nameof(Floorplan))]
        [Column("floorplan_id")]
        public Guid? FloorplanId { get; set; }

        [ForeignKey(nameof(Floor))]
        [Column("floor_id")]
        public Guid? FloorId { get; set; }

        [Column("is_active")]
        public int IsActive { get; set; } = 1;

        [Column("status")]
        public int Status { get; set; } = 1;

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public MstFloorplan? Floorplan { get; set; }
        public MstFloor? Floor { get; set; }
    }
}
