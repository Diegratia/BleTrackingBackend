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
    public class MonitoringConfigProfile : Profile
    {
        public MonitoringConfigProfile()
        {
            CreateMap<MonitoringConfig, MonitoringConfigDto>();
            CreateMap<MonitoringConfigCreateDto, MonitoringConfig>();
            CreateMap<MonitoringConfigUpdateDto, MonitoringConfig>();
        }
    }
}