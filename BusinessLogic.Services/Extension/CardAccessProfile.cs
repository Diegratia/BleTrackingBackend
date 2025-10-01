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
    public class CardAccessProfile : Profile
    {
        public CardAccessProfile()
        {
            // ========================
            // Card
            // ========================
            // CreateMap<Card, CardDto>();

            // ========================
            // FloorplanMaskedArea
            // ========================
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();

            // ========================
            // CardAccess
            // ========================
            // CreateMap<CardAccess, CardAccessDto>()
            //     .ForMember(dest => dest.MaskedAreaIds, 
            //         opt => opt.MapFrom(src => src.CardAccessMaskedAreas.Select(x => (Guid?)x.MaskedAreaId).ToList()))
            //     .ForMember(dest => dest.TimeGroupIds, 
            //         opt => opt.MapFrom(src => src.CardAccessTimeGroups.Select(x => (Guid?)x.TimeGroupId).ToList()));

    CreateMap<CardAccess, CardAccessDto>()
    .ForMember(dest => dest.MaskedAreaIds, 
        opt => opt.MapFrom(src => src.CardAccessMaskedAreas.Select(x => (Guid?)x.MaskedAreaId)))
    .ForMember(dest => dest.TimeGroupIds, 
        opt => opt.MapFrom(src => src.CardAccessTimeGroups.Select(x => (Guid?)x.TimeGroupId)));





            CreateMap<CardAccessCreateDto, CardAccess>()
            .ForMember(dest => dest.AccessScope, opt => opt.MapFrom(src => Enum.Parse<AccessScope>(src.AccessScope, true)))
                .ForMember(dest => dest.CardAccessMaskedAreas,
                    opt => opt.MapFrom(src => src.MaskedAreaIds.Select(id => new CardAccessMaskedArea { MaskedAreaId = id.Value })))
                .ForMember(dest => dest.CardAccessTimeGroups,
                    opt => opt.MapFrom(src => src.TimeGroupIds.Select(id => new CardAccessTimeGroups { TimeGroupId = id.Value })));


            CreateMap<CardAccessUpdateDto, CardAccess>()
            .ForMember(dest => dest.AccessScope, opt => opt.MapFrom(src => Enum.Parse<AccessScope>(src.AccessScope, true)))
                .ForMember(dest => dest.CardAccessMaskedAreas,
                    opt => opt.MapFrom(src => src.MaskedAreaIds.Select(id => new CardAccessMaskedArea { MaskedAreaId = id.Value })))
                .ForMember(dest => dest.CardAccessTimeGroups,
                    opt => opt.MapFrom(src => src.TimeGroupIds.Select(id => new CardAccessTimeGroups { TimeGroupId = id.Value })));
                

            // ========================
            // CardAccessMaskedArea (pivot)
            // ========================
            // CreateMap<CardAccessMaskedArea, CardAccessMaskedAreaDto>();
            // CreateMap<CardAccessMaskedAreaCreateDto, CardAccessMaskedArea>();

            // ========================
            // CardGroup
            // ========================
            // CreateMap<CardGroup, CardGroupDto>()
            //     .ForMember(dest => dest.Cards, opt => opt.MapFrom(src => src.Card))
            //     .ForMember(dest => dest.CardAccesses, opt => opt.MapFrom(src => 
            //         src.CardGroupCardAccesses.Select(cgca => cgca.CardAccess)));

            // CreateMap<CardGroupCreateDto, CardGroup>()
            //     .ForMember(dest => dest.CardGroupCardAccesses, opt => opt.MapFrom(src =>
            //         src.CardAccessIds.Select(id => new CardGroupCardAccess { CardAccessId = id })))
            //     .ForMember(dest => dest.Card, opt => opt.MapFrom(src =>
            //         src.CardIds.Select(id => new Card { Id = id })));

            // CreateMap<CardGroupUpdateDto, CardGroup>()
            //     .ForMember(dest => dest.CardGroupCardAccesses, opt => opt.MapFrom(src =>
            //         src.CardAccessIds.Select(id => new CardGroupCardAccess { CardAccessId = id })))
            //     .ForMember(dest => dest.Card, opt => opt.MapFrom(src =>
            //         src.CardIds.Select(id => new Card { Id = id })));

            // ========================
            // CardGroupCardAccess (pivot)
            // ========================
            // CreateMap<CardGroupCardAccess, CardGroupCardAccessDto>();
            // CreateMap<CardGroupCardAccessCreateDto, CardGroupCardAccess>();
        }
    }
}
