using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;
using Helpers.Consumer;

namespace BusinessLogic.Services.Extension
{
  public class FloorplanDeviceProfile : Profile
    {
        public FloorplanDeviceProfile()
        {
            CreateMap<FloorplanDeviceCreateDto, FloorplanDevice>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<DeviceType>(src.Type)))
                .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => Enum.Parse<DeviceStatus>(src.DeviceStatus)))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<FloorplanDeviceUpdateDto, FloorplanDevice>()
                 .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<DeviceType>(src.Type)))
                .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => Enum.Parse<DeviceStatus>(src.DeviceStatus)))
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<FloorplanDevice, FloorplanDeviceDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus.ToString()))
                .ForMember(dest => dest.Floorplan, opt => opt.Ignore()) // Diisi di service
                .ForMember(dest => dest.AccessCctv, opt => opt.Ignore()) // Diisi di service
                .ForMember(dest => dest.Reader, opt => opt.Ignore()) // Diisi di service
                .ForMember(dest => dest.AccessControl, opt => opt.Ignore()) // Diisi di service
                .ForMember(dest => dest.FloorplanMaskedArea, opt => opt.Ignore()); // Diisi di service
        }
    }
}
