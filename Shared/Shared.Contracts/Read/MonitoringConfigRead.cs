using System;

namespace Shared.Contracts.Read
{
    public class MonitoringConfigRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string Config { get; set; }
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public Guid ApplicationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
