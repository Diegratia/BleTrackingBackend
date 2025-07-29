using System;
using System.Collections.Generic;
using System.Linq;
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
        public string? QRCode { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedArea { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; } 
        public Guid? MemberId { get; set; } 
        public bool? StatusCard { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CardCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? QRCode { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedArea { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; } 
        public Guid? MemberId { get; set; } 
        public int? StatusCard { get; set; }
    }

    public class CardUpdateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? CardType { get; set; }
        public string? CardNumber { get; set; }
        public string? QRCode { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedArea { get; set; } // isikan  null jika bisa digunakan disemua site.
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; } 
        public Guid? MemberId { get; set; } 
        public int? StatusCard { get; set; }
    }
}


 