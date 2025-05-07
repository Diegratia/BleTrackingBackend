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
            CreateMap<VisitorBlacklistAreaCreateDto, VisitorBlacklistArea>();
            CreateMap<VisitorBlacklistAreaUpdateDto, VisitorBlacklistArea>();
            CreateMap<VisitorBlacklistArea, VisitorBlacklistAreaDto>();
            CreateMap<VisitorBlacklistAreaDto, VisitorBlacklistArea>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
            CreateMap<Visitor, VisitorDto>();
        }
        
    }
}