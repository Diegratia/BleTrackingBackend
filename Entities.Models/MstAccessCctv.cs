using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using System.Diagnostics.CodeAnalysis;

namespace Entities.Models
{
    public class MstAccessCctv : BaseModelWithTime, IApplicationEntity
    {
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Column("rtsp")]
        public string Rtsp { get; set; }

        [AllowNull]
        [ForeignKey("Integration")]
        [Column("integration_id")]
        public Guid? IntegrationId { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstIntegration Integration { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}