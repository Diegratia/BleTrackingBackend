using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;


namespace Data.ViewModels
{
    public class DashboardSummaryDto : BaseModelDto
    {
        public int ActiveBeaconCount { get; set; }
        public int NonActiveBeaconCount { get; set; }
        public int ActiveGatewayCount { get; set; }
        public int AreaCount { get; set; }
        public int BlacklistCount { get; set; }
        public int AlarmCount { get; set; }
        public List<CardDashboardDto> TopActiveBeacon { get; set; } = new List<CardDashboardDto>();
        public List<CardDashboardDto> TopNonActiveBeacon { get; set; } = new List<CardDashboardDto>();
        public List<ReaderSummaryDto> TopReaders { get; set; } = new List<ReaderSummaryDto>();
        public List<AreaSummaryDto> TopAreas { get; set; } = new List<AreaSummaryDto>();
        public List<AlarmTriggersSummary> TopTriggers { get; set; } = new List<AlarmTriggersSummary>();
    }

    public class CardUsageCountDto 
    {
         public int TotalCardCount { get; set; }
        public int VisitorCardCount { get; set; }
        public int MemberCardCount { get; set; }
        public int TotalCardUse { get; set; }
    }
}