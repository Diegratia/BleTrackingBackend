using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstEngine : IApplicationEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

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
            
        public virtual MstApplication Application { get; set; }
    }
}