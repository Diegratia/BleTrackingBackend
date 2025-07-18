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
        public float PointX { get; set; }
        public float PointY { get; set; }
        public float FirstReaderX { get; set; }
        public float FirstReaderY { get; set; }
        public float SecondReaderX { get; set; }
        public float SecondReaderY { get; set; }
        public DateTime Time { get; set; }
        public Guid FloorplanId { get; set; }
    }
}