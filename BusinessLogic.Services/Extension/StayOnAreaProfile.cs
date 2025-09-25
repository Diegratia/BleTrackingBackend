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
    public class StayOnAreaProfile : Profile
    {
        public StayOnAreaProfile()
        {
            CreateMap<StayOnArea, StayOnAreaDto>();
            CreateMap<StayOnAreaCreateDto, StayOnArea>();
            CreateMap<StayOnAreaUpdateDto, StayOnArea>();
        }
    }
}