using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{
    [Index(nameof(OriginalCardId))]
    [Index(nameof(SwappedCardId))]
    [Index(nameof(TrxVisitorId))]
    [Index(nameof(CreatedAt))]
    public class CardSwapTransaction : BaseModelWithTime, IApplicationEntity
    {
        [ForeignKey(nameof(OriginalCard))]
        [Column("original_card_id")]
        public Guid OriginalCardId { get; set; }

        [ForeignKey(nameof(SwappedCard))]
        [Column("swapped_card_id")]
        public Guid SwappedCardId { get; set; }

        [ForeignKey(nameof(TrxVisitor))]
        [Column("trx_visitor_id")]
        public Guid? TrxVisitorId { get; set; }

        [ForeignKey(nameof(Visitor))]
        [Column("visitor_id")]
        public Guid VisitorId { get; set; }

        [Column("swap_type")] 
        public SwapType SwapType { get; set; }

        [ForeignKey(nameof(MaskedArea))]
        [Column("masked_area_id")]
        public Guid MaskedAreaId { get; set; }

        [Column("swap_by")]
        public string SwapBy { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        // [Column("swap_chain_id")]
        // public Guid SwapChainId { get; set; } // semua swap dalam 1 session pakai id yang sama

        // [Column("swap_sequence")]
        // public int SwapSequence { get; set; } = 0; // urutan swap, layer swap

        [Column("is_active")]
        public bool IsActive { get; set; } = true; // Swap aktif terakhir = true

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; } // Untuk rollback

        public Card OriginalCard { get; set; }
        public Card SwappedCard { get; set; }
        public TrxVisitor? TrxVisitor { get; set; }
        public Visitor Visitor { get; set; }
        public FloorplanMaskedArea MaskedArea { get; set; }
        public MstApplication Application { get; set; }
    }


}