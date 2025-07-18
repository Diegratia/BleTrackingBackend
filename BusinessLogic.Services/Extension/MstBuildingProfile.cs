using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class MstBuildingProfile : Profile
    {
        public MstBuildingProfile()
        {
            // Mapping dari Domain ke DTO
            CreateMap<MstBuilding, MstBuildingDto>();

            // Mapping dari Create DTO ke Domain
            CreateMap<MstBuildingCreateDto, MstBuilding>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());

            // Mapping dari Update DTO ke Domain
            CreateMap<MstBuildingUpdateDto, MstBuilding>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
        }
    }
}