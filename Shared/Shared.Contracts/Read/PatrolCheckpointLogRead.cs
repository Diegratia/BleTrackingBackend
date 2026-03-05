using System;
using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class PatrolCheckpointLogRead
    {
        public Guid Id { get; set; }
        public Guid? PatrolAreaId { get; set; }
        public string? AreaNameSnap { get; set; }
        public int? OrderIndex { get; set; }
        public PatrolCheckpointStatus CheckpointStatus { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public int? MinDwellTime { get; set; }
        public int? MaxDwellTime { get; set; }
        public double? DistanceFromPrevMeters { get; set; }
        public string? Notes { get; set; }
    }
}
