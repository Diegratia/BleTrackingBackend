using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class CardRecordDto
    {
        public Guid Id { get; set; }
        public string? VisitorName { get; set; }
        public Guid CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
        public string CheckinBy { get; set; } = "";
        public string CheckoutBy { get; set; } = "";
        public Guid? CheckoutSiteId { get; set; }
        public Guid? CheckinSiteId { get; set; }
        public string? VisitorType { get; set; }
        public VisitorCardDto VisitorCard { get; set; }
        public VisitorDto Visitor { get; set; }
        public MstMemberDto Member { get; set; }
    }

    public class CardRecordCreateDto
    {
        public string? VisitorName { get; set; }
        public Guid CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? CheckoutSiteId { get; set; }
        public Guid? CheckinSiteId { get; set; }
        public string? VisitorType { get; set; }
    }

        public class CardRecordUpdateDto
    {
        public string? VisitorName { get; set; }
        public Guid CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? CheckoutSiteId { get; set; }
        public Guid? CheckinSiteId { get; set; }
        public string? VisitorType { get; set; }
    }
}