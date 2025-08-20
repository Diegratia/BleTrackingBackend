using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Helpers.Consumer;

namespace Entities.Models
{
    public class Card : BaseModelWithTime, IApplicationEntity
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
        [ForeignKey(nameof(RegisteredMaskedArea))]
        [Column("registered_masked_area_id")]
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua area.

        [Column("is_used")]
        public bool? IsUsed { get; set; }

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

        [Column("checkin_at")]
        public DateTime? CheckinAt { get; set; }

        [Column("checkout_at")]
        public DateTime? CheckoutAt { get; set; }

        [Column("status_card")]
        public int? StatusCard { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }
        // isikan  null jika bisa digunakan disemua area.

        public virtual MstApplication Application { get; set; }
        public virtual MstMember Member { get; set; }
        public virtual Visitor Visitor { get; set; }
        public virtual FloorplanMaskedArea RegisteredMaskedArea { get; set; }
        public virtual ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
    }
}



