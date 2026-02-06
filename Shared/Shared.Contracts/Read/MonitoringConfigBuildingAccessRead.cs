using System;

namespace Shared.Contracts.Read
{
    public class MonitoringConfigBuildingAccessRead
    {
        public Guid Id { get; set; }
        public Guid MonitoringConfigId { get; set; }
        public string? MonitoringConfigName { get; set; }
        public Guid BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public Guid ApplicationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public int Status { get; set; }
    }
}
