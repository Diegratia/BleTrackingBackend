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
    public class CardGroupProfile : Profile
    {
        public CardGroupProfile()
        {

            // CreateMap<CardGroup, CardGroupDto>();
            CreateMap<CardGroup, CardGroupDto>()
                .ForMember(dest => dest.CardAccesses,
                        opt => opt.MapFrom(src => src.CardGroupCardAccesses.Select(cga => cga.CardAccess)));

            CreateMap<CardGroupCreateDto, CardGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AccessScope, opt => opt.MapFrom(src => Enum.Parse<AccessScope>(src.AccessScope, true)));

            CreateMap<CardGroupUpdateDto, CardGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AccessScope, opt => opt.MapFrom(src => Enum.Parse<AccessScope>(src.AccessScope, true)));

            CreateMap<Card, CardDto>();
            CreateMap<Card, CardMinimalDto>();
            CreateMap<CardAccess, CardAccessDto>();


            // ========================
            // CardGroup
            // ========================
            // CreateMap<CardGroup, CardGroupDto>()
            //     .ForMember(dest => dest.Cards, opt => opt.MapFrom(src => src.Cards))
            //     .ForMember(dest => dest.CardAccesses, opt => opt.MapFrom(src => 
            //         src.CardGroupCardAccesses.Select(cgca => cgca.CardAccess)));

            // CreateMap<CardGroupCreateDto, CardGroup>()
            //     .ForMember(dest => dest.CardGroupCardAccesses, opt => opt.MapFrom(src =>
            //         src.CardAccessIds.Select(id => new CardGroupCardAccess { CardAccessId = id })))
            //     .ForMember(dest => dest.Cards, opt => opt.MapFrom(src =>
            //         src.CardIds.Select(id => new Card { Id = id.Value })));

            // CreateMap<CardGroupUpdateDto, CardGroup>()
            //     .ForMember(dest => dest.CardGroupCardAccesses, opt => opt.MapFrom(src =>
            //         src.CardAccessIds.Select(id => new CardGroupCardAccess { CardAccessId = id.Value })))
            //     .ForMember(dest => dest.Cards, opt => opt.MapFrom(src =>
            //         src.CardIds.Select(id => new Card { Id = id.Value })));

            // ========================
            // CardGroupCardAccess (pivot)
            // ========================
            // CreateMap<CardGroupCardAccess, CardGroupCardAccessDto>();
            // CreateMap<CardGroupCardAccessCreateDto, CardGroupCardAccess>();
        }
    }
}
