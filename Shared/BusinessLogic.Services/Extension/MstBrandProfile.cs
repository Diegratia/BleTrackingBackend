using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.ViewModels;
using Entities.Models;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Extension
{
    public class MstBrandProfile : Profile
    {
        public MstBrandProfile()
        {
            CreateMap<MstBrand, MstBrandRead>();
            CreateMap<MstBrand, MstBrandDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<MstBrand, OpenMstBrandDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<MstBrandCreateDto, MstBrand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
            CreateMap<MstBrandUpdateDto, MstBrand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}