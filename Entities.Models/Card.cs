using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{
    [Index(nameof(Dmac))]
    [Index(nameof(CardNumber))]
    public class Card : BaseModelWithTime, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("type")]
        public CardType? CardType { get; set; }
        [Column("card_status")]
        public CardStatus? CardStatus { get; set; }

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
        public bool? IsUsed { get; set; } = false;

        //WIP
        // [Column("is_swapped")]
        // public bool? IsSwapped { get; set; } = false;

        // [Column("swapped_with_card_id")]
        // public Guid? SwappedWithCardId { get; set; }

        // [Column("swapped_at")]
        // public DateTime? SwappedAt { get; set; }

        // [Column("current_swap_transaction_id")]
        // public Guid? CurrentSwapTransactionId { get; set; }

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
        [AllowNull]
        [ForeignKey(nameof(Security))]
        [Column("security_id")]
        public Guid? SecurityId { get; set; }

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

        [ForeignKey(nameof(CardGroup))]
        [Column("card_group_id")]
        public Guid? CardGroupId { get; set; }

        public MstApplication Application { get; set; }
        public MstMember Member { get; set; }
        public MstSecurity Security { get; set; }
        public Visitor Visitor { get; set; }
        public FloorplanMaskedArea RegisteredMaskedArea { get; set; }
        public CardGroup CardGroup { get; set; }
        public ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
        public ICollection<CardCardAccess?> CardCardAccesses { get; set; } = new List<CardCardAccess?>();
        [NotMapped]
        public ICollection<CardAccess> CardAccesses => CardCardAccesses.Select(cga => cga.CardAccess).ToList();
        // public Card? SwappedWithCard { get; set; }
        // public CardSwapTransaction? CurrentSwapTransaction { get; set; }
    }
}



