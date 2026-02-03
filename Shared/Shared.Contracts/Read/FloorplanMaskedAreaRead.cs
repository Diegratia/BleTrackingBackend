using System;

namespace Shared.Contracts.Read
{
    public class FloorplanMaskedAreaRead : BaseRead
    {
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string? Name { get; set; }
        public string? AreaShape { get; set; }
        public string? ColorArea { get; set; }
        public string? RestrictedStatus { get; set; }
        public bool AllowFloorChange { get; set; }
        public MstFloorRead? Floor { get; set; }
        public MstFloorplanRead? Floorplan { get; set; }
    }
}
