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
    public class AlarmCategorySettingsProfile : Profile
    {
        public AlarmCategorySettingsProfile()
        {
            CreateMap<AlarmCategorySettings, AlarmCategorySettingsDto>();
            CreateMap<AlarmCategorySettingsCreateDto, AlarmCategorySettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AlarmCategory, opt => opt.MapFrom(src => Enum.Parse<AlarmRecordStatus>(src.AlarmCategory, true)))
                .ForMember(dest => dest.AlarmLevelPriority, opt => opt.MapFrom(src => Enum.Parse<AlarmLevelPriority>(src.AlarmLevelPriority, true)));

            CreateMap<AlarmCategorySettingsUpdateDto, AlarmCategorySettings>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.AlarmCategory, opt => opt.MapFrom(src => Enum.Parse<AlarmRecordStatus>(src.AlarmCategory, true)))
                .ForMember(dest => dest.AlarmLevelPriority, opt => opt.MapFrom(src => Enum.Parse<AlarmLevelPriority>(src.AlarmLevelPriority, true)));
        }
       
    }
}