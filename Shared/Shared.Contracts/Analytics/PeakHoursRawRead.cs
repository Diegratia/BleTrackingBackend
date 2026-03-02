namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Raw DTO for peak hours data from SQL query.
    /// Used for internal mapping from database results.
    /// </summary>
    public class PeakHoursRawRead
    {
        /// <summary>
        /// Name of the entity based on GroupByMode (AreaName, BuildingName, FloorName, or FloorplanName)
        /// </summary>
        public string? Name { get; set; }
        public int Hour { get; set; }
        public int Count { get; set; }
    }
}
