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
    public class CardRecordProfile : Profile
    {
        public CardRecordProfile()
        {
            CreateMap<CardRecordCreateDto, CardRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VisitorType, opt => opt.MapFrom(src => Enum.Parse<VisitorType>(src.VisitorType)))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CheckinAt, opt => opt.Ignore())
                .ForMember(dest => dest.CheckoutAt, opt => opt.Ignore())
                .ForMember(dest => dest.CheckinBy, opt => opt.Ignore())
                .ForMember(dest => dest.CheckoutBy, opt => opt.Ignore());

            CreateMap<CardRecordUpdateDto, CardRecord>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VisitorType, opt => opt.MapFrom(src => Enum.Parse<VisitorType>(src.VisitorType)))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<CardRecord, CardRecordDto>()
                .ForMember(dest => dest.VisitorType, opt => opt.MapFrom(src => src.VisitorType.ToString()));

            CreateMap<Visitor, VisitorDto>();
            CreateMap<MstMember, MstMemberDto>();
            CreateMap<Card, CardDto>();
        }
    }
}
