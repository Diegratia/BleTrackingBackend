using System;

namespace Data.ViewModels
{

    public class TrackingTransactionDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public float? CoordinatePxX { get; set; }
        public float? CoordinatePxY { get; set; }
        public string? AlarmStatus { get; set; }
        public long? Battery { get; set; }
        public MstBleReaderDto? Reader { get; set; }
        public FloorplanMaskedAreaDto? FloorplanMaskedArea { get; set; }
        public CardDto? Card { get; set; }
    }

    public class TrackingTransactionCreateDto : BaseModelDto
    {
             public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public float? CoordinatePxX { get; set; }
        public float? CoordinatePxY { get; set; }
        public string? AlarmStatus { get; set; }
        public long? Battery { get; set; }
    }

    public class TrackingTransactionUpdateDto
    {
        public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public float? CoordinatePxX { get; set; }
        public float? CoordinatePxY { get; set; }
        public string? AlarmStatus { get; set; }
        public long? Battery { get; set; }
    }
}