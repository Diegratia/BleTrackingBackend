using AutoMapper;
using Entities.Models;
// using Data.ViewModels.TrackingAnalytics;
using Data.ViewModels;
using Repositories.Repository.RepoModel;
using Data.ViewModels.AlarmAnalytics;

namespace BusinessLogic.Services.Extension.Analytics
{
    public class TrackingAnalyticsProfile : Profile
    {
        public TrackingAnalyticsProfile()
        {
            // Mapping dari entity utama ke DTO (untuk log/detail alarm analytics)
            CreateMap<TrackingAnalyticsRequestDto, TrackingAnalyticsRequestRM>();

            // Mapping relasi ke DTO bawaan
            CreateMap<Visitor, VisitorDto>();
            CreateMap<MstBleReader, MstBleReaderDto>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();

            // NOTE: Mappings for TrackingAreaSummaryRM, TrackingDailySummaryRM, TrackingBuildingSummaryRM,
            // TrackingVisitorSummaryRM, and TrackingReaderSummaryRM have been removed
            // because repositories now return Read DTOs directly (TrackingAreaRead, TrackingDailyRead, etc.)
            // These Dtos are now redundant and can be deprecated in favor of Read DTOs.

            // Keep these mappings for now (still used by other features)
            CreateMap<TrackingPermissionCountRM, TrackingPermissionCountDto>();
            CreateMap<TrackingAccessPermissionSummaryRM, TrackingAccessPermissionSummaryDto>();
        }
    }
}
