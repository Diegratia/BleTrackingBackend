using System;
using Bogus.DataSets;

namespace Repositories.Repository.RepoModel
{
     public class TrackingTransactionRM
    {
        public Guid Id { get; set; }
        public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public string ReaderName { get; set; }
        public Guid? VisitorId { get; set; }
        public string VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string MemberName { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string FloorplanMaskedAreaName { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public string AlarmStatus { get; set; }
        public string ActionStatus { get; set; }
        public string AlarmRecprdStatus { get; set; }
        public long? Battery { get; set; }
    }
}
