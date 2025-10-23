namespace Data.ViewModels
{
    public class DataTablesRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string SortColumn { get; set; }
        public string SortDir { get; set; }
        public string SearchValue { get; set; }
         public string? TimeReport { get; set; } // <-- "Daily", "Weekly", "Monthly", "Yearly", "CustomDate"
        public string Mode { get; set; } = "table"; 
        public Dictionary<string, DateRangeFilter> DateFilters { get; set; } = new Dictionary<string, DateRangeFilter>();
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    }

    public class DateRangeFilter
    {
        
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}