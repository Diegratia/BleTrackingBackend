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
                .ForMember(dest => dest.VisitorType, opt => opt.MapFrom(src => src.VisitorType.ToString()));

            CreateMap<Visitor, VisitorDto>();
            CreateMap<MstMember, MstMemberDto>();
            CreateMap<Card, CardDto>();
        }
    }
}
