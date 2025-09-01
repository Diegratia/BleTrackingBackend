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
    public class AlarmTriggersProfile : Profile
    {
        public AlarmTriggersProfile()
        {

            CreateMap<AlarmTriggers, AlarmTriggersDto>()
               .ForMember(dest => dest.ActionStatus, opt => opt.MapFrom(src => src.Action.ToString()))
                .ForMember(dest => dest.AlarmRecordStatus, opt => opt.MapFrom(src => src.Alarm.ToString()));
            CreateMap<AlarmTriggersUpdateDto, AlarmTriggers>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => Enum.Parse<ActionStatus>(src.ActionStatus, true)));
            
        }
    }
}
