using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstAccessCctv : BaseModel
    {

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Rtsp { get; set; }

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [ForeignKey("Integration")]
        public Guid IntegrationId { get; set; }

        [Required]
        [ForeignKey("Application")]
        public Guid ApplicationId { get; set; }

        [Required]
        public int? Status { get; set; } = 1;

        public virtual MstIntegration Integration { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}