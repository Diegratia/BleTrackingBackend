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
    public class MstBleReaderProfile : Profile
    {
        public MstBleReaderProfile()
        {
            CreateMap<MstBleReader, MstBleReaderDto>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ReaderType, opt => opt.MapFrom(src => src.ReaderType.ToString()));
            CreateMap<MstBleReader, OpenMstBleReaderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ReaderType, opt => opt.MapFrom(src => src.ReaderType.ToString()));
            CreateMap<MstBleReaderCreateDto, MstBleReader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReaderType, opt => opt.MapFrom(src => Enum.Parse<ReaderType>(src.ReaderType, true)))
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<MstBleReaderUpdateDto, MstBleReader>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReaderType, opt => opt.MapFrom(src => Enum.Parse<ReaderType>(src.ReaderType, true)))
                .ForMember(dest => dest.Generate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<MstBrand, MstBrandDto>();
        }
    }
}