using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models 
{
    public class MstAccessControl : BaseModel
    {

        [Required]
        [ForeignKey("Brand")]
        public Guid ControllerBrandId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Type { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(255)]
        public string Channel { get; set; }

        [Required]
        [StringLength(255)]
        public string DoorId { get; set; } // relasi dari table?

        [Required]
        public string Raw { get; set; }

        [Required]
        [ForeignKey("Integration")]
        public Guid IntegrationId { get; set; }

        [Required]
        [ForeignKey("Application")]
        public Guid ApplicationId { get; set; }

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public string UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public int? Status { get; set; } = 1;

        public virtual MstApplication Application { get; set; }
        public virtual MstBrand Brand { get; set; }
        public virtual MstIntegration Integration { get; set; }
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}