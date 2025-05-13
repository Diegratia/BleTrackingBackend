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
            CreateMap<FloorplanDevice, FloorplanDeviceDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.AccessCctv, opt => opt.MapFrom(src => src.AccessCctv)) // Sesuaikan nama properti
                .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => src.DeviceStatus.ToString()))
                .ForMember(dest => dest.Floorplan, opt => opt.MapFrom(src => src.Floorplan))
                .ForMember(dest => dest.AccessCctv, opt => opt.MapFrom(src => src.AccessCctv))
                .ForMember(dest => dest.Reader, opt => opt.MapFrom(src => src.Reader))
                .ForMember(dest => dest.AccessControl, opt => opt.MapFrom(src => src.AccessControl))
                .ForMember(dest => dest.FloorplanMaskedArea, opt => opt.MapFrom(src => src.FloorplanMaskedArea));

       
            CreateMap<FloorplanDeviceCreateDto, FloorplanDevice>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<DeviceType>(src.Type, true)))
                .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => Enum.Parse<DeviceStatus>(src.DeviceStatus, true)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

         
            CreateMap<FloorplanDeviceUpdateDto, FloorplanDevice>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<DeviceType>(src.Type, true)))
                .ForMember(dest => dest.DeviceStatus, opt => opt.MapFrom(src => Enum.Parse<DeviceStatus>(src.DeviceStatus, true)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

           
            CreateMap<MstFloorplan, MstFloorplanDto>();
            CreateMap<MstAccessCctv, MstAccessCctvDto>(); 
            CreateMap<MstBleReader, MstBleReaderDto>();
            CreateMap<MstAccessControl, MstAccessControlDto>();
            CreateMap<FloorplanMaskedArea, FloorplanMaskedAreaDto>();
        }
    }
}