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
    public class VisitorCardProfile : Profile
    {
        public VisitorCardProfile()
        {
            CreateMap<VisitorCardCreateDto, VisitorCard>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => Enum.Parse<CardType>(src.CardType)))
                .ForMember(dest => dest.CheckinStatus, opt => opt.Ignore())
                .ForMember(dest => dest.EnableStatus, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.IsMember, opt => opt.Ignore());

            CreateMap<VisitorCardUpdateDto, VisitorCard>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => Enum.Parse<CardType>(src.CardType)))
                .ForMember(dest => dest.CheckinStatus, opt => opt.Ignore()) 
                .ForMember(dest => dest.EnableStatus, opt => opt.Ignore()) 
                .ForMember(dest => dest.Status, opt => opt.Ignore()) 
                .ForMember(dest => dest.IsMember, opt => opt.Ignore()); 

            CreateMap<VisitorCard, VisitorCardDto>()
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType.ToString()));
        }
    }
}
