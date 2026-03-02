using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class MstIntegrationProfile : Profile
    {
        public MstIntegrationProfile()
        {
            CreateMap<MstIntegration, MstIntegrationDto>()
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                 .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand));
            CreateMap<MstIntegrationCreateDto, MstIntegration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<MstIntegrationUpdateDto, MstIntegration>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<MstIntegrationDto, MstIntegration>();
            CreateMap<MstBrand, MstBrandDto>();
          
        }
    }
}