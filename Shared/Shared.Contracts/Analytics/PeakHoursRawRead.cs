namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Raw DTO for peak hours data from SQL query.
    /// Used for internal mapping from database results.
    /// </summary>
    public class PeakHoursRawRead
    {
        public string? AreaName { get; set; }
        public int Hour { get; set; }
        public int Count { get; set; }
    }
}
