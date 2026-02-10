using System;
using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Area summary for alarm analytics
    /// </summary>
    public class AlarmAreaRead
    {
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AlarmStatus { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    /// <summary>
    /// Visitor summary for alarm analytics
    /// </summary>
    public class AlarmVisitorRead
    {
        public Guid? VisitorId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    /// <summary>
    /// Building summary for alarm analytics
    /// </summary>
    public class AlarmBuildingRead
    {
        public Guid? BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    /// <summary>
    /// Daily summary for alarm analytics
    /// </summary>
    public class AlarmDailyRead
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
    }

    /// <summary>
    /// Status summary for alarm analytics
    /// </summary>
    public class AlarmStatusRead
    {
        public string Status { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    /// <summary>
    /// Hourly status summary for alarm analytics
    /// </summary>
    public class AlarmHourlyStatusRead
    {
        public int Hour { get; set; }
        public string HourLabel { get; set; } = string.Empty;
        public Dictionary<string, int> Status { get; set; } = new();
    }

    /// <summary>
    /// Individual entity chart series data.
    /// EntityId and Name are populated based on GroupByMode (Area, Building, Floor, or Floorplan).
    /// </summary>
    public class AlarmAreaSeriesRead
    {
        public Guid? EntityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<global::Shared.Contracts.ChartSeriesDto> Series { get; set; } = new();
    }

    /// <summary>
    /// Full area chart response
    /// </summary>
    public class AlarmAreaChartResponseRead
    {
        public List<string> Labels { get; set; } = new();
        public List<AlarmAreaSeriesRead> Areas { get; set; } = new();
    }

    /// <summary>
    /// Summary statistics for area access
    /// </summary>
    public class AlarmAreaSummaryRead
    {
        public int AccessedAreaTotal { get; set; }
        public int WithPermission { get; set; }
        public int WithoutPermission { get; set; }
    }

    /// <summary>
    /// Area daily aggregate for alarm analytics
    /// </summary>
    public class AlarmAreaDailyRead
    {
        public DateTime Date { get; set; }
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AlarmStatus { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
