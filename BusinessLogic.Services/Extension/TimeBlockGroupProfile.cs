using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class TimeBlockGroupProfile : Profile
    {
        public TimeBlockGroupProfile()
        {
            // TimeGroup
            CreateMap<TimeGroup, TimeGroupDto>();
            CreateMap<TimeGroupCreateDto, TimeGroup>();
            CreateMap<TimeGroupUpdateDto, TimeGroup>();

            // TimeBlock
CreateMap<TimeBlock, TimeBlockDto>()
    .ForMember(dest => dest.DayOfWeek,
        opt => opt.MapFrom(src => src.DayOfWeek.HasValue ? src.DayOfWeek.Value.ToString() : null));

CreateMap<TimeBlockCreateDto, TimeBlock>()
    .ForMember(dest => dest.DayOfWeek,
        opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DayOfWeek)
            ? Enum.Parse<DayOfWeek>(src.DayOfWeek, true)
            : (DayOfWeek?)null));

CreateMap<TimeBlockUpdateDto, TimeBlock>()
    .ForMember(dest => dest.DayOfWeek,
        opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DayOfWeek)
            ? Enum.Parse<DayOfWeek>(src.DayOfWeek, true)
            : (DayOfWeek?)null));
        }
    }
}