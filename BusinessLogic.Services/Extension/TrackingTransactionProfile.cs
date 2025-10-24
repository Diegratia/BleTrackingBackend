using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;
using Helpers.Consumer;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Extension
{
    public class TrackingTransactionProfile : Profile
    {
        public TrackingTransactionProfile()
        {
            // CreateMap<TrackingTransactionCreateDto, TrackingTransaction>()
            //     .ForMember(dest => dest.AlarmStatus, opt => opt.MapFrom(src => Enum.Parse<AlarmStatus>(src.AlarmStatus, true)));

            // CreateMap<TrackingTransactionUpdateDto, TrackingTransaction>()
            //     .ForMember(dest => dest.AlarmStatus, opt => opt.MapFrom(src => Enum.Parse<AlarmStatus>(src.AlarmStatus, true)));

            CreateMap<TrackingTransaction, TrackingTransactionDto>();
            CreateMap<MstBleReader, MstBleReaderDto>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
            CreateMap<TrackingTransactionWithAlarm, TrackingTransactionWithAlarmDto>();

            CreateMap<TrackingTransaction, TrackingTransactionRM>()
           .ForMember(dest => dest.ReaderName, opt => opt.MapFrom(src => src.Reader.Name))
           .ForMember(dest => dest.VisitorName, opt => opt.MapFrom(src => src.Visitor.Name))
           .ForMember(dest => dest.FloorplanMaskedAreaName, opt => opt.MapFrom(src => src.FloorplanMaskedArea.Name));

        }
    }
}



