using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class AlarmRecordTrackingProfile : Profile
    {
        public AlarmRecordTrackingProfile()
        {
            CreateMap<AlarmRecordTracking, AlarmRecordTrackingDto>()
                .ForMember(dest => dest.Visitor, opt => opt.MapFrom(src => src.Visitor))
                .ForMember(dest => dest.Reader, opt => opt.MapFrom(src => src.Reader))
                .ForMember(dest => dest.ActionStatus, opt => opt.MapFrom(src => src.Action.ToString()))
                .ForMember(dest => dest.AlarmRecordStatus, opt => opt.MapFrom(src => src.Alarm.ToString()))
                .ForMember(dest => dest.FloorplanMaskedArea, opt => opt.MapFrom(src => src.FloorplanMaskedArea));

            CreateMap<AlarmRecordTrackingCreateDto, AlarmRecordTracking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());

            CreateMap<AlarmRecordTrackingUpdateDto, AlarmRecordTracking>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
               

            CreateMap<Visitor, VisitorDto>();
            CreateMap<MstBleReader, MstBleReaderDto>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
        }
    }
}