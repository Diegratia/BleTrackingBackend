namespace Data.ViewModels.AlarmAnalytics
{
    public class AlarmAreaSummaryDto
    {
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string AlarmStatus { get; set; } = string.Empty;
        public int Total { get; set; }
    }

        public class AlarmAreaChartCollectionDto
        {
            public AlarmAreaChartDto Chart { get; set; } = new();
        }


        public class AlarmAreaChartDto
        {
            public List<string> Labels { get; set; } = new();
            public List<AlarmAreaChartAreaDto> Areas { get; set; } = new();
        }


            public class AlarmAreaChartAreaDto
        {
            public Guid AreaId { get; set; }
            public string AreaName { get; set; } = string.Empty;
            public List<AlarmAreaStatusSeriesDto> Series { get; set; } = new();
        }

        public class AlarmAreaStatusSeriesDto
    {
        public string Name { get; set; } = string.Empty; // Blacklist / CardAccess
        public List<int> Data { get; set; } = new();
    }





    public class AlarmVisitorSummaryDto
    {
        public Guid? VisitorId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmBuildingSummaryDto
    {
        public Guid? BuildingId { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmDailySummaryDto
    {
        public DateTime Date { get; set; }
        public int Total { get; set; }
    }

    public class AlarmStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Total { get; set; }
    }

    public class AlarmHourlySummaryRM
    {
        public int Hour { get; set; } // 0–23
        public int Total { get; set; }
        public string? Status { get; set; }
    }

     public class AlarmHourlyStatusSummaryDto
    {
        public int Hour { get; set; }
        public string HourLabel { get; set; } = string.Empty;
        public Dictionary<string, int> Status { get; set; } = new();
        // public int Total { get; set; }
    }

}
