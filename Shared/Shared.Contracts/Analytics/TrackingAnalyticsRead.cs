using System;
using System.Collections.Generic;

namespace Shared.Contracts.Analytics
{
    /// <summary>
    /// Area summary for tracking analytics
    /// </summary>
    public class TrackingAreaRead
    {
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Daily summary for tracking analytics
    /// </summary>
    public class TrackingDailyRead
    {
        public DateTime Date { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Building summary for tracking analytics
    /// </summary>
    public class TrackingBuildingRead
    {
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Visitor summary for tracking analytics
    /// </summary>
    public class TrackingVisitorRead
    {
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Card summary for tracking analytics
    /// </summary>
    public class TrackingCardRead
    {
        public Guid? PersonId => VisitorId ?? MemberId;
        public string? PersonName => VisitorName ?? MemberName;
        public string PersonType =>
                    VisitorId.HasValue ? "Visitor" :
                    MemberId.HasValue ? "Member" :
                    "Unknown";
        public string? IdentityId { get; set; }
        public Guid? CardId { get; set; }
        public string? CardName { get; set; }
        public string? CardNumber { get; set; }
        public Guid? VisitorId { get; set; }
        public string? VisitorName { get; set; }
        public Guid? MemberId { get; set; }
        public string? MemberName { get; set; }

        // Location info (last detected)
        public Guid? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public Guid? FloorId { get; set; }
        public string? FloorName { get; set; }
        public Guid? FloorplanId { get; set; }
        public string? FloorplanName { get; set; }
        public string? FloorplanImage { get; set; }
        public Guid? AreaId { get; set; }
        public string? AreaName { get; set; }
        public float LastX { get; set; }
        public float LastY { get; set; }

        public DateTime? LastDetectedAt { get; set; }
    }

    /// <summary>
    /// Reader summary for tracking analytics
    /// </summary>
    public class TrackingReaderRead
    {
        public Guid? ReaderId { get; set; }
        public string? ReaderName { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Movement tracking for a specific card
    /// </summary>
    public class TrackingMovementRead
    {
        public Guid CardId { get; set; }
        public string? PersonName { get; set; }
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Floorplan { get; set; }
        public string? Area { get; set; }

        public DateTime? EnterTime { get; set; }
        public DateTime? ExitTime { get; set; }

        public List<TrackingPositionPointRead> Positions { get; set; } = new();
    }

    /// <summary>
    /// Position point for tracking movement
    /// </summary>
    public class TrackingPositionPointRead
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    /// <summary>
    /// Heatmap data for tracking analytics
    /// </summary>
    public class TrackingHeatmapRead
    {
        public Guid FloorplanId { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int Count { get; set; }
    }

    // NOTE: AreaAccessAggregateRow is kept in Repositories.Repository.RepoModel
    // as it's only used internally within the repository for SQL query mapping
}
