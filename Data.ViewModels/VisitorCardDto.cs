using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class VisitorCardDto
    {
        public Guid Id { get; set; }
        public long Generate { get; set; }
        public string? Name { get; set; } = "";
        public string? Number { get; set; } = "";
        public string? CardType { get; set; }
        public string? QRCode { get; set; } = "";
        public string? mac { get; set; }
        public int? CheckinStatus { get; set; }
        public int? EnableStatus { get; set; }
        public int Status { get; set; }
        public Guid? SiteId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid CardId { get; set; }
        public int IsVisitor { get; set; }
        public Guid ApplicationId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public MstMemberDto Member { get; set; }
        public VisitorDto Visitor { get; set; }
        public CardDto Card { get; set; }
    }

    public class VisitorCardCreateDto
    {
        public string? Name { get; set; } = "";
        public string? Number { get; set; } = "";
        public string? CardType { get; set; }
        public string? QRCode { get; set; } = "";
        public string? mac { get; set; }
        public Guid? SiteId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid CardId { get; set; }
        public Guid ApplicationId { get; set; }
    }

     public class VisitorCardUpdateDto
    {
        public string? Name { get; set; } = "";
        public string? Number { get; set; } = "";
        public string? CardType { get; set; }
        public string? QRCode { get; set; } = "";
        public string? mac { get; set; }
        public Guid? SiteId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid CardId { get; set; }
        public Guid ApplicationId { get; set; }
    }
}