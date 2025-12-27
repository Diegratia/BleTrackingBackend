using System;
using Bogus.DataSets;

namespace Repositories.Repository.RepoModel
{
    public class TrackingTransactionRM
    {
        public Guid Id { get; set; }
        public DateTime? TransTime { get; set; }
        public Guid? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }
        public Guid? FloorplanMaskedAreaId { get; set; }
        public string? FloorplanMaskedAreaName { get; set; }
        public string? AreaShape { get; set; }
        public float? CoordinateX { get; set; }
        public float? CoordinateY { get; set; }
        public string AlarmStatus { get; set; }
        public string ActionStatus { get; set; }
        public string AlarmRecprdStatus { get; set; }
        public long? Battery { get; set; }
    }

    public class TrackingAnalyticsRequestRM
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? TimeRange { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? CardId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public Guid? ReaderId { get; set; }

    }

    // RepoModel/VisitorSessionSummaryRM.cs
    public class VisitorSessionSummaryRM
    {
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        // public Guid? MemberId { get; set; }
        // public string? MemberName { get; set; }
        public Guid? CardId { get; set; }
        public string? CardName { get; set; }

        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public Guid? FloorId { get; set; }
        public string? FloorName { get; set; }
        public Guid? FloorplanId { get; set; }
        public string? FloorplanName { get; set; }
        public string? FloorplanImage { get; set; }
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        // public Guid? PersonId { get; set; }
        // public string? PersonName { get; set; }
        public string? PersonType { get; set; }  // "Visitor" atau "Employee"

        public DateTime EnterTime { get; set; }     // WIB
        public DateTime? ExitTime { get; set; }     // WIB (null = masih di dalam)
        public int? DurationInMinutes { get; set; } // Exit - Enter

        public string? Status { get; set; }         // Checkin / Block / dll
        public string? HostName { get; set; }
    }

    public class TrackingAreaSummaryRM
    {
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingDailySummaryRM
    {
        public DateTime Date { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingBuildingSummaryRM
    {
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingVisitorSummaryRM
    {
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingCardSummaryRM
    {
        public Guid? CardId { get; set; }
        public string? CardName { get; set; }

        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }

        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }

        // public int TotalRecords { get; set; }

        public DateTime? EnterTime { get; set; }
        public DateTime? ExitTime { get; set; }

        // Lokasi terakhir
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public string? FloorplanImage { get; set; }
        public Guid? FloorId { get; set; }
        public string? FloorName { get; set; }
        public Guid? FloorplanId { get; set; }
        public string? FloorplanName { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public string? MaskedAreaName { get; set; }
        public float? LastX { get; set; }
        public float? LastY { get; set; }
    }



    public class TrackingReaderSummaryRM
    {
        public Guid? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public int TotalRecords { get; set; }
    }

    public class TrackingMovementRM
    {
        public Guid CardId { get; set; }
        public string? PersonName { get; set; }
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Floorplan { get; set; }
        public string? Area { get; set; }

        public DateTime? EnterTime { get; set; }
        public DateTime? ExitTime { get; set; }

        public List<TrackingPositionPointRM> Positions { get; set; } = new();
    }

    public class TrackingPositionPointRM
    {
        public float X { get; set; }
        public float Y { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is not TrackingPositionPointRM other)
                return false;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
            => HashCode.Combine(X, Y);
    }

    public class TrackingHeatmapRM
    {
        public Guid FloorplanId { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int Count { get; set; }
    }

    public class TrackingAccessPermissionSummaryRM
    {
        public int AccessedAreaTotal { get; set; }
        public int WithPermission { get; set; }
        public int WithoutPermission { get; set; }
    }

    public class CardUsageCountRM
    {
        public int TotalCardCount { get; set; }
        public int VisitorCardCount { get; set; }
        public int MemberCardCount { get; set; }
        public int TotalCardUse { get; set; }
    }


    public class TrackingPermissionCountRM
    {
        public Guid? AreaId { get; set; }
        public int WithPermission { get; set; }
        public int WithoutPermission { get; set; }
        public int TotalRecords { get; set; }
        public string AreaName { get; set; }

    }


    //count hell
            public class TrackingHierarchyRM
        {
            public Dictionary<string, TrackingBuildingNode> Buildings { get; set; } = new();
        }

        public class TrackingBuildingNode
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public int Count { get; set; }
            public Dictionary<string, TrackingFloorNode> Floors { get; set; } = new();
        }

        public class TrackingFloorNode
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public int Count { get; set; }
            public Dictionary<string, TrackingFloorplanNode> Floorplans { get; set; } = new();
        }

        public class TrackingFloorplanNode
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public int Count { get; set; }
            public Dictionary<string, TrackingAreaNode> Areas { get; set; } = new();
        }

        public class TrackingAreaNode
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public int Count { get; set; }
        }




}
