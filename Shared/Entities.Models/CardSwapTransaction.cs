using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace Entities.Models
{
    [Index(nameof(FromCard))]
    [Index(nameof(ToCard))]
    [Index(nameof(ExecutedAt))]
    [Index(nameof(SwapChainId))]
    [Index(nameof(SwapSequence))]
    public class CardSwapTransaction : IApplicationEntity
    {

        [Required]
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("from_card_id")]
        public Guid FromCardId { get; set; }

        [Column("to_card_id")]
        public Guid ToCardId { get; set; }

        [Column("trx_visitor_id")]
        public Guid? TrxVisitorId { get; set; }

        [Column("visitor_id")]
        public Guid VisitorId { get; set; }

        [Column("swap_type")]
        public SwapType? SwapType { get; set; }

        [Column("card_swap_status")]
        public CardSwapStatus? CardSwapStatus { get; set; }

        [Column("masked_area_id")]
        public Guid? MaskedAreaId { get; set; }

        [Column("swap_by")]
        public string? SwapBy { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("swap_chain_id")]
        public Guid? SwapChainId { get; set; } // semua swap dalam 1 session pakai id yang sama

        [Column("swap_sequence")]
        public int SwapSequence { get; set; } = 0; // urutan swap, layer swap

        // 🔑 identity context (WAJIB)
        [Column("identity_type")]
        public IdentityType? IdentityType { get; set; }

        [Column("identity_value")]
        public string? IdentityValue { get; set; } // FREE INPUT (NIK / Badge / NDA / dll)

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; } // reverse at

        [Column("executed_at")]
        public DateTime? ExecutedAt { get; set; } // action

        public Card? FromCard { get; set; }
        public Card? ToCard { get; set; }
        public TrxVisitor? TrxVisitor { get; set; }
        public Visitor? Visitor { get; set; }
        public FloorplanMaskedArea? MaskedArea { get; set; }
        public MstApplication Application { get; set; }
        
    }


}