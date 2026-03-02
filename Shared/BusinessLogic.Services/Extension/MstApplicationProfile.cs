using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class MstApplicationProfile : Profile
    {
        public MstApplicationProfile()
        {
            CreateMap<MstApplication, MstApplicationDto>()
            .ForMember(dest => dest.ApplicationStatus, opt => opt.MapFrom(src => src.ApplicationStatus));

            CreateMap<MstApplicationCreateDto, MstApplication>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationRegistered, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<MstApplicationUpdateDto, MstApplication>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationRegistered, opt => opt.Ignore());

        }
    }
}



