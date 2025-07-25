using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Helpers.Consumer;

namespace Entities.Models
{
    public class Card : BaseModelWithTime
    {
        [Column("name")]
        public string? Name { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("type")]
        public CardType? CardType { get; set; }

        [Column("card_number")]
        public string? CardNumber { get; set; }

        [Column("qr_code")]
        public string? QRCode { get; set; }

        [Column("dmac")]
        public string? Dmac { get; set; }

        [Column("is_multi_masked_area")]
        public bool? IsMultiMaskedArea { get; set; }

        [AllowNull]
        [ForeignKey(nameof(FloorplanMaskedArea))]
        [Column("registered_masked_area")]
        public Guid? RegisteredMaskedArea { get; set; } // isikan  null jika bisa digunakan disemua area.

        [Column("is_used")]
        public bool? IsUsed { get; set; } = false;

        [Column("last_used_by")]
        public string? LastUsed { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Visitor))]
        [Column("visitor_id")]
        public Guid? VisitorId { get; set; } 
 
        [AllowNull]
        [ForeignKey(nameof(Member))]
        [Column("member_id")]
        public Guid? MemberId { get; set; } 

        [Column("checked_in_at")]
        public DateTime? CheckedInAt { get; set; }
        [Column("checked_out_at")]
        public DateTime? CheckedOutAt { get; set; }

        [Column("status_card")]
        public bool? StatusCard { get; set; } = true;
        // isikan  null jika bisa digunakan disemua area.

        public virtual MstMember Member { get; set; }
        public virtual Visitor Visitor { get; set; }
        public virtual FloorplanMaskedArea RegisteredMaskedAreas { get; set; }
    }
}



