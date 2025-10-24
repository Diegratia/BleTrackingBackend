using System;

namespace Repositories.Repository.RepoModel
{
    public class TrackingTransactionRM
    {
        public Guid Id { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? ReaderName { get; set; }
        public string? VisitorName { get; set; }
        public string? MaskedAreaName { get; set; }
        public string? AlarmStatus { get; set; }
        public string? ActionStatus { get; set; }
    }
}
