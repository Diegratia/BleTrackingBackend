using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Response DTO for Peak Hours by Area chart.
    /// Shows hourly distribution of visitors across different areas.
    /// </summary>
    public class PeakHoursByAreaRead
    {
        /// <summary>
        /// Hour labels (e.g., "06:00", "07:00", "08:00", etc.)
        /// Typically 24 hours for a full day view
        /// </summary>
        public List<string> Labels { get; set; } = new();

        /// <summary>
        /// One series per area showing visitor count per hour
        /// </summary>
        public List<ChartSeriesDto> Series { get; set; } = new();
    }
}
