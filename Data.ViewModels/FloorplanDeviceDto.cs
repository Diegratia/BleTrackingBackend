using System;

namespace Data.ViewModels
{
    public class FloorplanDeviceDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid AccessCctvId { get; set; }
        public Guid ReaderId { get; set; }
        public Guid AccessControlId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosPxX { get; set; }
        public float PosPxY { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public Guid ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string DeviceStatus { get; set; }
        public int? Status { get; set; }

        public MstFloorplanDto Floorplan { get; set; }
        public MstAccessCctvDto AccessCctv { get; set; }
        public MstBleReaderDto Reader { get; set; }
        public MstAccessControlDto AccessControl { get; set; }
        public FloorplanMaskedAreaDto FloorplanMaskedArea { get; set; }
        
    }

     public class FloorplanDeviceCreateDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid AccessCctvId { get; set; }
        public Guid ReaderId { get; set; }
        public Guid AccessControlId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosPxX { get; set; }
        public float PosPxY { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public Guid ApplicationId { get; set; }
        public string DeviceStatus { get; set; }
    }

     public class FloorplanDeviceUpdateDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid AccessCctvId { get; set; }
        public Guid ReaderId { get; set; }
        public Guid AccessControlId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosPxX { get; set; }
        public float PosPxY { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public Guid ApplicationId { get; set; }
        public string DeviceStatus { get; set; }
    }
}