using System;

namespace Shared.Contracts.Read
{
    public class MstFloorplanRead : BaseRead
    {
        public string? Name { get; set; }
        public Guid FloorId { get; set; }
        public string? FloorplanImage { get; set; }
        public float PixelX { get; set; }
        public float PixelY { get; set; }
        public float FloorX { get; set; }
        public float FloorY { get; set; }
        public float MeterPerPx { get; set; }
        public Guid? EngineId { get; set; }
        public int MaskedAreaCount { get; set; }
        public int DeviceCount { get; set; }
        public int? PatrolAreaCount { get; set; }
        public MstFloorRead? Floor { get; set; }
    }
}
