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

            CreateMap<CardGroup, CardGroupDto>();
            CreateMap<CardGroupCreateDto, CardGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<CardGroupUpdateDto, CardGroup>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Card, CardDto>();
            CreateMap<Card, CardMinimalDto>();
            CreateMap<CardAccess, CardAccessDto>();
        }
    }
}
