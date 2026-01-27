using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;
using Shared.Contracts;

namespace BusinessLogic.Services.Extension
{
    public class AlarmTriggersProfile : Profile
    {
        public AlarmTriggersProfile()
        {

            CreateMap<AlarmTriggers, AlarmTriggersDto>()
                .ForMember(dest => dest.ActionStatus, opt => opt.MapFrom(src => src.Action.ToString()))
                .ForMember(dest => dest.AssignedSecurityId, opt => opt.MapFrom(src => src.SecurityId))
                .ForMember(dest => dest.AlarmRecordStatus, opt => opt.MapFrom(src => src.Alarm.ToString()));
            CreateMap<AlarmTriggers, AlarmTriggersOpenDto>()
                .ForMember(dest => dest.ActionStatus, opt => opt.MapFrom(src => src.Action.ToString()))
                .ForMember(dest => dest.AlarmRecordStatus, opt => opt.MapFrom(src => src.Alarm.ToString()));
            CreateMap<AlarmTriggersUpdateDto, AlarmTriggers>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(x => x.SecurityId, opt => opt.Ignore())
                .ForMember(x => x.InvestigatedBy, opt => opt.Ignore())
                .ForMember(x => x.InvestigatedTimestamp, opt => opt.Ignore())
                .ForMember(x => x.ActionUpdatedAt, opt => opt.Ignore())
                .ForMember(x => x.DoneBy, opt => opt.Ignore())
                .ForMember(x => x.DoneTimestamp, opt => opt.Ignore())
                .ForMember(x => x.CancelBy, opt => opt.Ignore())
                .ForMember(x => x.CancelTimestamp, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityId, opt => opt.MapFrom(src => src.AssignedSecurityId))
                .ForMember(dest => dest.Action, opt => opt.MapFrom(src => Enum.Parse<ActionStatus>(src.ActionStatus, true)));
            
        }
    }
}
