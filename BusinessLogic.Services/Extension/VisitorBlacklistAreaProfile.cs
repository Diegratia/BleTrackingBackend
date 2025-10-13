using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class VisitorBlacklistAreaProfile : Profile
    {

        public VisitorBlacklistAreaProfile()
        {
            CreateMap<VisitorBlacklistAreaCreateDto, BlacklistArea>();
            CreateMap<VisitorBlacklistAreaUpdateDto, BlacklistArea>();
            CreateMap<BlacklistArea, VisitorBlacklistAreaDto>();
            CreateMap<BlacklistArea, OpenVisitorBlacklistAreaDto>();
            CreateMap<VisitorBlacklistAreaDto, BlacklistArea>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
            CreateMap<Visitor, VisitorDto>();
        }
        
    }
}