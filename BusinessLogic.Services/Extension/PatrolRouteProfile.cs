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
            // CreateMap<PatrolRoute, PatrolRouteDto>()
            // .ForMember(dest => dest.PatrolAreaIds,
            //     opt => opt.MapFrom(src =>
            //         src.PatrolRouteAreas
            //             .Where(x => x.status != 0)
            //             .OrderBy(x => x.OrderIndex)
            //             .Select(x => (Guid?)x.PatrolAreaId)
            //             .ToList()
            //     ));
            CreateMap<PatrolRoute, PatrolRouteDto>()
            .ForMember(dest => dest.PatrolAreas,
                opt => opt.MapFrom(src =>
                    src.PatrolRouteAreas
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => new PatrolRouteAreaDto
                        {
                            PatrolAreaId = x.PatrolAreaId,
                            OrderIndex = x.OrderIndex,
                            EstimatedDistance = x.EstimatedDistance,
                            EstimatedTime = x.EstimatedTime,
                            StartAreaId = x.StartAreaId,
                            EndAreaId = x.EndAreaId
                        })
                        .ToList()
                ))
            .ForMember(dest => dest.PatrolTimeGroups,
                opt => opt.MapFrom(src =>
                    src.PatrolRouteTimeGroups
                        // .OrderBy(x => x.OrderIndex)
                        .Select(x => new PatrolTimeGroupDto
                        {
                            TimeGroupId = x.TimeGroupId,
                            Name = x.TimeGroup.Name,
                            ScheduleType = x.TimeGroup.ScheduleType.ToString(),
                        })
                        .ToList()
                ));
            // .ForMember(dest => dest.TimeGroupIds,
            //         opt => opt.MapFrom(src =>
            //             src.PatrolRouteTimeGroups
            //                 .Where(x => x.Status != 0)
            //                 .Select(x => (Guid?)x.TimeGroupId)
            //                 .ToList()
            //         ));

            CreateMap<PatrolRouteRM, PatrolRouteDto>();
            CreateMap<PatrolRouteLookUpRM, PatrolRouteDto>();

            // CreateMap<PatrolRouteRM, PatrolRouteDto>();
            CreateMap<PatrolRouteLookUpRM, PatrolRouteLookUpDto>();
            CreateMap<PatrolRouteCreateDto, PatrolRoute>();
            CreateMap<PatrolRouteUpdateDto, PatrolRoute>();
        }
    }
}