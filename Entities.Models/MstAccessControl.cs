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
    public class MstAccessControl : BaseModelWithTime, IApplicationEntity
    {
        [AllowNull]
        [ForeignKey("Brand")]
        [Column("controller_brand_id")]
        public Guid? BrandId { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Column("type")]
        public string? Type { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Column("channel")]
        public string? Channel { get; set; }

        [Required]
        [StringLength(255)]
        [Column("door_id")]
        public string DoorId { get; set; } // relasi dari table?

        [Column("raw")]
        public string? Raw { get; set; }

        [ForeignKey("Integration")]
        [Column("integration_id")]
        public Guid? IntegrationId { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; } = 1;

        public virtual MstApplication Application { get; set; }
        public virtual MstBrand Brand { get; set; }
        public virtual MstIntegration Integration { get; set; }
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}