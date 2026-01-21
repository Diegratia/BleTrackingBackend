using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.RepoModel
{
    public class CardUsageHistory
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Guid VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public string? CardNumber { get; set; }
    }
     public class CardUsageSummaryRM
    {
        public Guid? CardId { get; set; }
        public string CardNumber { get; set; }
        public int TotalUsage { get; set; }
    }

    public class CardUsageHistoryRM
    {
        public Guid? CardId { get; set; }
        public string? IdentityId { get; set; }
        public string CardNumber { get; set; }
        public string UsedBy { get; set; }
        public string UsedByType { get; set; } // Visitor / Member
        public DateTime? CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }
    }

        public class CardRecordRequestRM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? ReaderId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? OperatorName { get; set; }
        public bool? IsActive { get; set; }
        public string? PersonType { get; set; }
        public string? ReportTitle { get; set; }
        public string? ExportType { get; set; }
    }

        public class CardRecordListRM
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public string? CardNumber { get; set; }
        public string? PersonName { get; set; }
        public string? IdentityId { get; set; }
        public string? PersonType { get; set; }
        public DateTime CheckinAt { get; set; }
        public DateTime? CheckoutAt { get; set; }

        // derived (in-memory)
        public string Duration { get; set; }
        public string Status { get; set; }

        public DateTime UpdatedAt { get; set; } // wajib untuk sorting default
    }
}