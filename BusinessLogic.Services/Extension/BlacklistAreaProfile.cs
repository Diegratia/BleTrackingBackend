using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class BlacklistAreaProfile : Profile
    {

        public BlacklistAreaProfile()
        {
            CreateMap<BlacklistAreaCreateDto, BlacklistArea>();
            CreateMap<BlacklistAreaUpdateDto, BlacklistArea>();
            CreateMap<BlacklistArea, BlacklistAreaDto>();
            CreateMap<BlacklistArea, OpenBlacklistAreaDto>();
            CreateMap<BlacklistAreaDto, BlacklistArea>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
            CreateMap<Visitor, VisitorDto>();
        }
        
    }
}