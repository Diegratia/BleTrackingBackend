using System;

namespace Data.ViewModels
{

    public class TrackingTransactionDto
    {
        public Guid Id { get; set; }
        public DateTime TransTime { get; set; }
        public Guid ReaderId { get; set; }
        public long CardId { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public decimal CoordinateX { get; set; }
        public decimal CoordinateY { get; set; }
        public long CoordinatePxX { get; set; }
        public long CoordinatePxY { get; set; }
        public string AlarmStatus { get; set; } // Enum sebagai string
        public long Battery { get; set; }
        public MstBleReaderDto Reader { get; set; }
        public FloorplanMaskedAreaDto FloorplanMaskedArea { get; set; }
    }

    public class TrackingTransactionCreateDto
    {
        public DateTime TransTime { get; set; }
        public Guid ReaderId { get; set; }
        public long CardId { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public decimal CoordinateX { get; set; }
        public decimal CoordinateY { get; set; }
        public long CoordinatePxX { get; set; }
        public long CoordinatePxY { get; set; }
        public string AlarmStatus { get; set; } // Enum sebagai string
        public long Battery { get; set; }
    }

    public class TrackingTransactionUpdateDto
    {
        public DateTime TransTime { get; set; }
        public Guid ReaderId { get; set; }
        public long CardId { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
        public decimal CoordinateX { get; set; }
        public decimal CoordinateY { get; set; }
        public long CoordinatePxX { get; set; }
        public long CoordinatePxY { get; set; }
        public string AlarmStatus { get; set; } // Enum sebagai string
        public long Battery { get; set; }
    }
}