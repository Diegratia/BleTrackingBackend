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
    public class MstTrackingLogProfile : Profile
    {
        public MstTrackingLogProfile()
        {
            CreateMap<MstTrackingLog, MstTrackingLogDto>();
            CreateMap<MstTrackingLogDto, MstTrackingLog>();
        }
    }
}