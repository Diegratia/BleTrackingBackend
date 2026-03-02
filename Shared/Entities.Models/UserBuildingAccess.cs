using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("user_building_access")]
    public class UserBuildingAccess : BaseModelWithTimeApp, IApplicationEntity
    {
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("building_id")]
        public Guid BuildingId { get; set; }

        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        // Navigation properties
        public User User { get; set; }
        public MstBuilding Building { get; set; }
    }
}
