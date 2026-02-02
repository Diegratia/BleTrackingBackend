using System;

namespace Shared.Contracts.Read
{
    public class PatrolCheckpointLogRead
    {
        public Guid Id { get; set; }
        public Guid? PatrolAreaId { get; set; }
        public string? AreaNameSnap { get; set; }
        public int? OrderIndex { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public double? DistanceFromPrevMeters { get; set; }
    }
}
