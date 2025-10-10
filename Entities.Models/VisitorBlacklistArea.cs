using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class VisitorBlacklistArea : BaseModelWithTime, IApplicationEntity
    {
        [Required]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Required]
        [Column("visitor_id")]
        public Guid VisitorId { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        public FloorplanMaskedArea FloorplanMaskedArea { get; set; }
        public MstApplication Application { get; set;}
        public Visitor Visitor { get; set; }
    }
}