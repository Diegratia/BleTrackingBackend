using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class TrxVisitorProfile : Profile
    {
        public TrxVisitorProfile()
        {
            CreateMap<TrxVisitorCreateDto, TrxVisitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<TrxVisitorUpdateDto, TrxVisitor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<TrxVisitor, TrxVisitorDto>();
            CreateMap<BlockReasonDto, TrxVisitor>();
            CreateMap<DenyReasonDto, TrxVisitor>();
            CreateMap<TrxVisitorCheckinDto, TrxVisitor>();
            CreateMap<Visitor, VisitorDto>();
        }
        
    }
}