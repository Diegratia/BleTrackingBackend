using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;
using Helpers.Consumer;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Extension
{
    public class PatrolAreaProfile : Profile
    {
        public PatrolAreaProfile()
        {
            CreateMap<PatrolArea, PatrolAreaDto>();
            CreateMap<PatrolAreaRM, PatrolAreaDto>();
            CreateMap<PatrolAreaLookUpRM, PatrolAreaLookUpDto>();
            CreateMap<PatrolAreaCreateDto, PatrolArea>();
            CreateMap<PatrolAreaUpdateDto, PatrolArea>();
        }
    }
}