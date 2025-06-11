using System;

namespace Data.ViewModels
{
    public class MstTrackingLogDto
    {
        public Guid Id { get; set; }
        public string BeaconId { get; set; }
        public string Pair { get; set; }
        public string FirstReaderId { get; set; }
        public string SecondReaderId { get; set; }
        // public decimal FirstDist { get; set; }
        // public decimal SecondDist { get; set; }
        // public decimal JarakMeter { get; set; }
        public decimal PointX { get; set; }
        public decimal PointY { get; set; }
        public decimal FirstReaderX { get; set; }
        public decimal FirstReaderY { get; set; }
        public decimal SecondReaderX { get; set; }
        public decimal SecondReaderY { get; set; }
        public DateTime Time { get; set; }
        public Guid FloorplanId { get; set; }
    }
}