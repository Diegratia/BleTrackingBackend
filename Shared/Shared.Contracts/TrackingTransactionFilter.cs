using System;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class TrackingTransactionFilter : BaseFilter
    {
        public Guid? ReaderId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? AlarmStatus { get; set; }
    }
}
