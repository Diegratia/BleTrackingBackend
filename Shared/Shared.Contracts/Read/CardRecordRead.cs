using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class CardRecordRead : BaseRead
    {
        public string? Name { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public string? CheckinBy { get; set; }
        public string? CheckoutBy { get; set; }
        public Guid? CheckoutMaskedArea { get; set; }
        public Guid? CheckinMaskedArea { get; set; }
        public bool IsActive { get; set; }
        public VisitorActiveStatus? VisitorActiveStatus { get; set; }

        // Navigation - critical fields only
        public CardNavigationRead? Card { get; set; }
        public VisitorNavigationRead? Visitor { get; set; }
        public MemberNavigationRead? Member { get; set; }
    }

    public class CardNavigationRead
    {
        public Guid Id { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
    }

    public class VisitorNavigationRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? IdentityId { get; set; }
        public string? FaceImage { get; set; }
    }

    public class MemberNavigationRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? IdentityId { get; set; }
        public string? FaceImage { get; set; }
    }
}
