using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("monitoring_config_building_access")]
    public class MonitoringConfigBuildingAccess : BaseModelWithTimeApp, IApplicationEntity
    {
        [Required]
        [Column("monitoring_config_id")]
        public Guid MonitoringConfigId { get; set; }

        [Required]
        [Column("building_id")]
        public Guid BuildingId { get; set; }

        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        // Navigation properties
        public MonitoringConfig MonitoringConfig { get; set; }
        public MstBuilding Building { get; set; }
    }
}
