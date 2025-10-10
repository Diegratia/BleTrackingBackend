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
        [Column("integration_id")]
        public Guid? IntegrationId { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public MstIntegration Integration { get; set; }

        public MstApplication Application { get; set; }
        public ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}