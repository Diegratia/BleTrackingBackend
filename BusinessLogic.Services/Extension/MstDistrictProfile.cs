using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;

namespace BusinessLogic.Services.Extension
{
    public class MstDistrictProfile : Profile
    {
        public MstDistrictProfile()
        {
            CreateMap<MstDistrict, MstDistrictDto>() 
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<MstDistrict, OpenMstDistrictDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<MstDistrictCreateDto, MstDistrict>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
            CreateMap<MstDistrictUpdateDto, MstDistrict>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
        }  
                 
    }
}