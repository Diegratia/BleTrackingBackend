using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class CardDto : BaseModelDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CardGroupId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public bool? StatusCard { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        public VisitorDto? Visitor { get; set; } // Visitor
        public FloorplanMaskedAreaDto? RegisteredMaskedArea { get; set; } // Visitor
        public MstMemberDto? Member { get; set; } // Visitor
        public CardGroupDto? CardGroup { get; set; }
    }
    public class CardMinimalDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CardGroupId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public bool? StatusCard { get; set; }
        // public VisitorMinimalDto? Visitor { get; set; } // Visitor
        // public FloorplanMaskedAreaMinimalDto? RegisteredMaskedArea { get; set; } // Visitor
        // public MstMemberDto? Member { get; set; } // Visitor
    }
    public class CardMinimalsDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CardGroupId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public bool? StatusCard { get; set; }
        public List<CardAccessDto?> CardAccesses { get; set; } = new();

    }
    
            public class CardAddDto : BaseModelDto
        {
            public string? Name { get; set; }
            public string? Remarks { get; set; }
            public string? CardType { get; set; }
            public string? CardNumber { get; set; }
            public string? Dmac { get; set; }
            public bool? IsMultiMaskedArea { get; set; }
            public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
            public Guid? VisitorId { get; set; }
            public Guid? MemberId { get; set; }
            public Guid? CardGroupId { get; set; }
            public List<Guid?> CardAccessIds { get; set; } = new();
        }

        public class CardEditDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CardGroupId { get; set; }
        public List<Guid?> CardAccessIds { get; set; } = new();
    }

    public class CardAccessEdit : BaseModelDto
    {
        public List<Guid?> CardAccessIds { get; set; } = new();
    }
    
    public class OpenCardDto : BaseModelDto
    {
        public long Generate { get; set; }
        [JsonPropertyName("card_id")]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public bool? StatusCard { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // public VisitorDto? Visitor { get; set; } // Visitor
        // public FloorplanMaskedAreaDto? RegisteredMaskedArea { get; set; } // Visitor
        // public MstMemberDto? Member { get; set; } // Visitor
    }

    public class CardCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CardGroupId { get; set; }
    }

    public class CardUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? QRCode { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? CardGroupId { get; set; }
        public int? StatusCard { get; set; }
    }
}
