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
    public class CardProfile : Profile
    {
        public CardProfile()
        {
            CreateMap<CardCreateDto, Card>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => Enum.Parse<CardType>(src.CardType)))
                .ForMember(dest => dest.IsMultiSite, opt => opt.Ignore())
                .ForMember(dest => dest.IsUsed, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsed, opt => opt.Ignore())
                .ForMember(dest => dest.StatusCard, opt => opt.Ignore());

            CreateMap<CardUpdateDto, Card>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => Enum.Parse<CardType>(src.CardType)))
                .ForMember(dest => dest.IsMultiSite, opt => opt.Ignore())
                .ForMember(dest => dest.IsUsed, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsed, opt => opt.Ignore())
                .ForMember(dest => dest.StatusCard, opt => opt.Ignore());

            CreateMap<Card, CardDto>()
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType.ToString()));
        }
    }
}
