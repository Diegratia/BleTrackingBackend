using System;
using System.Text.Json.Serialization;
using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class CardRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public CardType? CardType { get; set; }
        public CardStatus? CardStatus { get; set; }
        public string? CardNumber { get; set; }
        public string? QRCode { get; set; }
        public string? Dmac { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; }
        public bool? IsUsed { get; set; }
        public string? LastUsed { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? SecurityId { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public int? StatusCard { get; set; }
        public Guid? CardGroupId { get; set; }

        // Navigation properties
        public string? MemberName { get; set; }
        public string? VisitorName { get; set; }
        public string? SecurityName { get; set; }
        public string? CardGroupName { get; set; }
        public string? RegisteredMaskedAreaName { get; set; }
        public List<CardAccessRead>? CardAccesses { get; set; }
    }
}
