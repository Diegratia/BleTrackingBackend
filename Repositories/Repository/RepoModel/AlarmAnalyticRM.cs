using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.RepoModel
{
    public class AlarmAnalyticRM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? OperatorName { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
    }

    public class AlarmDailySummaryRM
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
        public int Done { get; set; }
        public int Cancelled { get; set; }
        public double? AvgResponseSeconds { get; set; }
    }
    
     public class AlarmAnalyticsRequestRM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? OperatorName { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
    }
}