using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class MstFloorplanProfile : Profile
    {
        public MstFloorplanProfile()
        {
            CreateMap<MstFloorplan, MstFloorplanDto>()
           .ForMember(dest => dest.MaskedAreaCount, opt => opt.MapFrom(src => src.FloorplanMaskedAreas.Count(m => m.Status != 0)))
            .ForMember(dest => dest.DeviceCount, opt => opt.MapFrom(src => src.FloorplanDevices.Count(m => m.Status != 0)));

            CreateMap<MstFloorplanCreateDto, MstFloorplan>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<MstFloorplanUpdateDto, MstFloorplan>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
