using System;

namespace Shared.Contracts.Analytics
{
    public class SecurityHeadDashboardSummaryRead
    {
        public string AvgResponseTimeMetric { get; set; } = "00m 00s";
        public int CountAlarmToInvestigate { get; set; }
        public int CountPatrolAssignment { get; set; }
        public NextPatrolRead? NextPatrol { get; set; }
    }

    public class NextPatrolRead
    {
        public Guid AssignmentId { get; set; }
        public string AssignmentName { get; set; }
        public string TimeGroupName { get; set; }
        public string ScheduleStart { get; set; }
        public string ScheduleEnd { get; set; }
    }
}
