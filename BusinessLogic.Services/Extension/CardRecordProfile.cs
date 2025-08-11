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

            CreateMap<CardRecord, CardRecordDto>()
                .ForMember(dest => dest.VisitorActiveStatus, opt => opt.MapFrom(src => src.VisitorActiveStatus.ToString()));
            CreateMap<CardRecordCreateDto, CardRecordDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
            CreateMap<CardRecordUpdateDto, CardRecordDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());

            CreateMap<Visitor, VisitorDto>();
            CreateMap<MstMember, MstMemberDto>();
            CreateMap<Card, CardDto>();
        }
    }
}
