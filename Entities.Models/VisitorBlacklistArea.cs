using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class VisitorBlacklistArea : BaseModel, IApplicationEntity
    {
        [Required]
        [ForeignKey("FloorplanMaskedArea")]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Required]
        [ForeignKey("Visitor")]
        [Column("visitor_id")]
        public Guid VisitorId { get; set; }

        [Required]
        [ForeignKey("ApplicationId")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual FloorplanMaskedArea FloorplanMaskedArea { get; set; }
        public virtual MstApplication Application { get; set;}
        public virtual Visitor Visitor { get; set; }
    }
}