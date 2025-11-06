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
        [Column("controller_brand_id")]
        public Guid? BrandId { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Column("type")]
        public string? Type { get; set; }

        [Column("is_assigned")]
        public bool? IsAssigned { get; set; } = false;

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

        [Column("integration_id")]
        public Guid? IntegrationId { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; } = 1;

        public MstApplication Application { get; set; }
        public MstBrand Brand { get; set; }
        public MstIntegration Integration { get; set; }
        public ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}