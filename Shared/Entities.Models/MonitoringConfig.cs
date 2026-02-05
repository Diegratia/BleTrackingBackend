using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MonitoringConfig : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("config")]
        [Required]
        public string Config { get; set; }

        [Column("building_id")]
        public Guid? BuildingId { get; set; } 

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public MstApplication Application { get; set; }
        public MstBuilding? Building { get; set; }
    }
}