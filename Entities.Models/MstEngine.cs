using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;


namespace Entities.Models
{
    [Index(nameof(EngineId), IsUnique = true)]
    public class MstEngine : BaseModelOnlyIdWithTime,IApplicationEntity
    {
        [MaxLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [MaxLength(255)]
        [Column("engine_id")]
        public string? EngineId { get; set; }

        [Column("port")]
        public int? Port { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [Column("is_live")]
        public int? IsLive { get; set; }

        [Column("last_live")]
        public DateTime? LastLive { get; set; }

        [MaxLength(50)]
        [Column("service_status")]
        public ServiceStatus? ServiceStatus { get; set; }

        [Required]
        [ForeignKey(nameof(Application))]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public MstApplication Application { get; set; }
    }
}