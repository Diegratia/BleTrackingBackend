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
            CreateMap<TrackingAreaSummaryRM, TrackingAreaSummaryDto>();
            CreateMap<TrackingVisitorSummaryRM, TrackingVisitorSummaryDto>();
            CreateMap<TrackingBuildingSummaryRM, TrackingBuildingSummaryDto>();
            CreateMap<TrackingDailySummaryRM, TrackingDailySummaryDto>();
            CreateMap<TrackingReaderSummaryRM, TrackingReaderSummaryDto>();
            
        }
    }
}
