using System;

namespace Data.ViewModels
{
    public class RecordTrackingLogDto
    {
        public Guid Id { get; set; }
        public string TableName { get; set; }
        public Guid FloorplanId { get; set; }
        public DateTime FloorplanTimestamp { get; set; }
    }
}