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
    public class BoundaryProfile : Profile
    {
        public BoundaryProfile()
        {
            CreateMap<Boundary, BoundaryDto>();
            CreateMap<BoundaryCreateDto, Boundary>();
            CreateMap<BoundaryUpdateDto, Boundary>();
        }
    }
}