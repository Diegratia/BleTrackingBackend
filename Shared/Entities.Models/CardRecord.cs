using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using System.Diagnostics.CodeAnalysis;
using Shared.Contracts;

namespace Entities.Models
{
    public class CardRecord : BaseModelWithTime, IApplicationEntity
    {
        [Column("name")]
        public string Name { get; set; }

        [ForeignKey(nameof(Card))]
        [Column("card_id")]
        public Guid? CardId { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Visitor))]
        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Member))]
        [Column("member_id")]
        public Guid? MemberId { get; set; }

        // [Column("type")]
        // public CardType? Type { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("checkin_at")]
        public DateTime? CheckinAt { get; set; }

        [Column("checkout_at")]
        public DateTime? CheckoutAt { get; set; }

        [Column("checkin_by")]
        public string? CheckinBy { get; set; }

        [Column("checkout_by")]
        public string? CheckoutBy { get; set; }

        [Column("checkout_masked_area")]
        public Guid? CheckoutMaskedArea { get; set; }

        [Column("checkin_masked_area")]
        public Guid? CheckinMaskedArea { get; set; }

        [Column("visitor_type")]
        public VisitorActiveStatus? VisitorActiveStatus { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        // [ForeignKey(nameof(CardSwapTransaction))]
        // [Column("card_swap_transaction_id")]
        // public Guid? CardSwapTransactionId { get; set; }

        // [Column("is_swap_record")]
        // public bool IsSwapRecord { get; set; } = false;

        [NotMapped]
        public bool IsActive => CheckoutAt == null;



        public MstApplication Application { get; set; }
        public Visitor Visitor { get; set; }
        public MstMember Member { get; set; }
        public Card Card { get; set; }
        // public CardSwapTransaction? CardSwapTransaction { get; set; }
    }
}