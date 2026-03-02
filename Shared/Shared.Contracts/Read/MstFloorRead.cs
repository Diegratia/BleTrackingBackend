using System;

namespace Shared.Contracts.Read
{
    public class MstFloorRead : BaseRead
    {
        public Guid BuildingId { get; set; }
        public string? Name { get; set; }
        public MstBuildingRead? Building { get; set; }
    }
}
