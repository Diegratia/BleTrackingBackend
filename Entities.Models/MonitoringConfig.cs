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
        public string? Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public string Config { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

         public MstApplication Application { get; set; }
    }
}