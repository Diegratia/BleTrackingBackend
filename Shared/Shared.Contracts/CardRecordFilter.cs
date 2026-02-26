using System;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class CardRecordFilter : BaseFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public bool? IsActive { get; set; }
    }
}
