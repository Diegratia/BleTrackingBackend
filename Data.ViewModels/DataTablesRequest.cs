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
    }
}