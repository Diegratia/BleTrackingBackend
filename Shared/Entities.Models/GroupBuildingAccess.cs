using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("group_building_access")]
    public class GroupBuildingAccess : BaseModelWithTimeApp, IApplicationEntity
    {
        [Required]
        [Column("group_id")]
        public Guid GroupId { get; set; }

        [Required]
        [Column("building_id")]
        public Guid BuildingId { get; set; }

        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        // Navigation properties
        public UserGroup Group { get; set; }
        public MstBuilding Building { get; set; }
    }
}
