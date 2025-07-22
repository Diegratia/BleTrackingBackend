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
    public class VisitorCard : BaseModelWithTime
    {
        [Required]
        [Column("name")]
        public string? Name { get; set; } = "";

        [Column("number")]
        public string? Number { get; set; } = "";

        [Column("card_type")]
        public CardType? CardType { get; set; }

        [Column("qr_code")]
        public string? QRCode { get; set; } = "";

        [Column("mac")]
        public string? Mac { get; set; }

        [Column("card_id")]
        [ForeignKey(nameof(Card))]
        public Guid CardId { get; set; }

        [Column("checkin_status")]
        public int? CheckinStatus { get; set; }

        [Column("enable_status")]
        public int? EnableStatus { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("site_id")]
        public Guid? SiteId { get; set; }

        [Column("is_visitor")]
        public int? IsVisitor { get; set; }

        [ForeignKey(nameof(Application))]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Visitor))]
        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Member))]
        [Column("member_id")]
        public Guid? MemberId { get; set; }

        public virtual Card Card { get; set; }
        public virtual Visitor Visitor { get; set; }
        public virtual MstMember Member { get; set; }
        public virtual MstApplication Application { get; set; }
        public virtual ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
    }
}