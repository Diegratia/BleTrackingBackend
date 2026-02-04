using System;

namespace Shared.Contracts.Read
{
    public class FloorplanDeviceRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? DeviceStatus { get; set; }
        public float? PosX { get; set; }
        public float? PosY { get; set; }
        public float? PosPxX { get; set; }
        public float? PosPxY { get; set; }
        public Guid? ReaderId { get; set; }
        public Guid? AccessCctvId { get; set; }
        public Guid? AccessControlId { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? Path { get; set; }

        public MinimalFloorplanRead? Floorplan { get; set; }
        public MinimalBleReaderRead? Reader { get; set; }
        public MinimalAccessCctvRead? AccessCctv { get; set; }
        public MinimalAccessControlRead? AccessControl { get; set; }
        public MinimalMaskedAreaRead? FloorplanMaskedArea { get; set; }
    }

    public class MinimalFloorplanRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? FloorplanImage { get; set; }
    }

    public class MinimalBleReaderRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Ip { get; set; }
        public string? Gmac { get; set; }
        public Guid BrandId { get; set; }
    }

    public class MinimalAccessCctvRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Rtsp { get; set; }
        public Guid? IntegrationId { get; set; }
    }

    public class MinimalAccessControlRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? BrandId { get; set; }
        public string? Channel { get; set; }
    }

    public class MinimalMaskedAreaRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? ColorArea { get; set; }
        public string? AreaShape { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string? RestrictedStatus { get; set; }
    }
}
