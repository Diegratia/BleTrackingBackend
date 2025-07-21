using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;
using Helpers.Consumer;

namespace BusinessLogic.Services.Extension
{
    public class TrackingTransactionProfile : Profile
    {
        public TrackingTransactionProfile()
        {
            CreateMap<TrackingTransactionCreateDto, TrackingTransaction>()
                .ForMember(dest => dest.AlarmStatus, opt => opt.MapFrom(src => Enum.Parse<AlarmStatus>(src.AlarmStatus, true)));

            CreateMap<TrackingTransactionUpdateDto, TrackingTransaction>()
                .ForMember(dest => dest.AlarmStatus, opt => opt.MapFrom(src => Enum.Parse<AlarmStatus>(src.AlarmStatus, true)));

            CreateMap<TrackingTransaction, TrackingTransactionDto>();
            CreateMap<MstBleReader, MstBleReaderDto>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
        }
    }
}