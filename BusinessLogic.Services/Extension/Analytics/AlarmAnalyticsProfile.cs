using AutoMapper;
using Entities.Models;
using Data.ViewModels.AlarmAnalytics;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension.Analytics
{
    public class AlarmAnalyticsProfile : Profile
    {
        public AlarmAnalyticsProfile()
        {
            // Mapping dari entity utama ke DTO (untuk log/detail alarm analytics)
            CreateMap<AlarmRecordTracking, AlarmAnalyticsDto>()
                .ForMember(dest => dest.Visitor, opt => opt.MapFrom(src => src.Visitor))
                .ForMember(dest => dest.Reader, opt => opt.MapFrom(src => src.Reader))
                .ForMember(dest => dest.FloorplanMaskedArea, opt => opt.MapFrom(src => src.FloorplanMaskedArea))
                .ForMember(dest => dest.AlarmRecordStatus, opt => opt.MapFrom(src => src.Alarm.ToString()))
                .ForMember(dest => dest.ActionStatus, opt => opt.MapFrom(src => src.Action.ToString()));

            // Mapping relasi ke DTO bawaan
            CreateMap<Visitor, VisitorDto>();
            CreateMap<MstBleReader, MstBleReaderDto>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
        }
    }
}
