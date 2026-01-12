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
    public class PatrolRouteProfile : Profile
    {
        public PatrolRouteProfile()
        {
            CreateMap<PatrolRoute, PatrolRouteDto>()
            .ForMember(dest => dest.PatrolAreaIds,
                opt => opt.MapFrom(src =>
                    src.PatrolRouteAreas
                        .Where(x => x.status != 0)
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => (Guid?)x.PatrolAreaId)
                        .ToList()
                ));
            // CreateMap<PatrolRouteRM, PatrolRouteDto>();
            // CreateMap<PatrolRouteLookUpRM, PatrolRouteLookUpDto>();
            CreateMap<PatrolRouteCreateDto, PatrolRoute>();
            CreateMap<PatrolRouteUpdateDto, PatrolRoute>();
        }
    }
}