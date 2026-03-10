using System.Collections.Generic;

namespace Shared.Contracts
{
    /// <summary>
    /// Universal chart series DTO used across all analytics services.
    /// Compatible with Chart.js, ECharts, ApexCharts, Highcharts, and other frontend charting libraries.
    /// </summary>
    public class ChartSeriesDto
    {
        /// <summary>
        /// Series name (e.g., "Active", "Inactive", "With Permission", "Without Permission")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Data points for this series.
        /// Each value corresponds to a label in the chart's Labels array.
        /// </summary>
        public List<int> Data { get; set; } = new();

        /// <summary>
        /// Optional: Color for consistent frontend rendering across different chart instances.
        /// Frontend can use this to maintain consistent colors for specific series.
        /// Example: "#FF5733", "rgba(255, 87, 51, 1)"
        /// </summary>
        public string? Color { get; set; }

        // /// <summary>
        // /// Optional: Chart type override for this specific series.
        // /// Values: "line", "bar", "area", "pie", "doughnut"
        // /// When null, uses the chart's default type.
        // /// </summary>
        // public string? Type { get; set; }
    }

    /// <summary>
    /// Optional metadata for chart statistics.
    /// Use this to provide summary statistics along with chart data.
    /// </summary>
    public class ChartMetadataDto
    {
        /// <summary>
        /// Total sum of all data points across all series
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Average value across all data points
        /// </summary>
        public double Average { get; set; }

        /// <summary>
        /// Maximum value in the dataset
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// Minimum value in the dataset
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// Count of data points (number of labels/time periods)
        /// </summary>
        public int Count { get; set; }
    }
}
