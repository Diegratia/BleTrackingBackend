using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class BlacklistArea : BaseModelWithTime, IApplicationEntity
    {
        [Required]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }

        [Column("member_id")]
        public Guid? MemberId { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }
        public FloorplanMaskedArea FloorplanMaskedArea { get; set; }
        public MstApplication Application { get; set;}
        public Visitor Visitor { get; set; }
        public MstMember Member { get; set; }
    }
}