using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
   public class MstEngineProfile : Profile
    {
        public MstEngineProfile()
        {
            CreateMap<MstEngine, MstEngineDto>() 
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IsLive, opt => opt.MapFrom(src => src.IsLive));
            CreateMap<MstEngineCreateDto, MstEngine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<MstEngineUpdateDto, MstEngine>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
                 
        }
    }
}