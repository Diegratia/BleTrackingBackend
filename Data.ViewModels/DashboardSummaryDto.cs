using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}