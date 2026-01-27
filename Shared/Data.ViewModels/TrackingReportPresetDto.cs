// File: Data/ViewModels/TrackingReportPresetDTOs.cs
using System;
using System.Collections.Generic;

namespace Data.ViewModels
{
    /// <summary>
    /// Request untuk apply preset (sangat simple!)
    /// </summary>
    public class ApplyPresetRequest
    {
        public Guid PresetId { get; set; }
    }

    /// <summary>
    /// Request untuk bikin preset custom (hanya tanggal!)
    /// </summary>
    public class CreateCustomPresetRequest
    {
        public string Name { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TimeRange { get; set; } = "custom";
    }
    
        public class UpdatePresetRequest
    {
        public string Name { get; set; }

        public string TimeRange { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int? Status { get; set; }
    }

    /// <summary>
    /// Response untuk list preset
    /// </summary>
    public class PresetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TimeRange { get; set; }
        public Guid? BuildingId { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? MemberId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; }
    }
}