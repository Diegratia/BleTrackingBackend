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
             .ForMember(dest => dest.PatrolAreaCount,
                opt => opt.MapFrom(src =>
                    src.PatrolRouteAreas.Count(x => x.status != 0)))

            .ForMember(dest => dest.StartAreaName,
                opt => opt.MapFrom(src =>
                    src.PatrolRouteAreas
                        .Where(x => x.status != 0)
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault()
                ))

            .ForMember(dest => dest.EndAreaName,
                opt => opt.MapFrom(src =>
                    src.PatrolRouteAreas
                        .Where(x => x.status != 0)
                        .OrderByDescending(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault()
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

    public class PatrolAssignmentProfile : Profile
    {
        public PatrolAssignmentProfile()
        {
            CreateMap<PatrolAssignmentCreateDto, PatrolAssignment>()
                .ForMember(d => d.PatrolAssignmentSecurities, opt => opt.Ignore());

            CreateMap<PatrolAssignmentUpdateDto, PatrolAssignment>()
                .ForMember(d => d.PatrolAssignmentSecurities, opt => opt.Ignore());
            CreateMap<PatrolAssignmentLookUpRM, PatrolAssignmentLookUpDto>();
            CreateMap<PatrolAssignment, PatrolAssignmentDto>()
            .ForMember(dest => dest.Securities,
                opt => opt.MapFrom(src =>
                    src.PatrolAssignmentSecurities.Select(x => new SecurityListDto
                    {
                        Id = x.SecurityId,
                        Name = x.Security != null ? x.Security.Name : null,
                        CardNumber = x.Security != null ? x.Security.CardNumber : null,
                        IdentityId = x.Security != null ? x.Security.IdentityId : null,
                        OrganizationName = x.Security != null && x.Security.Organization != null
                            ? x.Security.Organization.Name
                            : null,
                        DepartmentName = x.Security != null && x.Security.Department != null
                            ? x.Security.Department.Name
                            : null,
                        DistrictName = x.Security != null && x.Security.District != null
                            ? x.Security.District.Name
                            : null,
                        ApplicationId = x.Security != null ? x.Security.ApplicationId : Guid.Empty
                    }).ToList()
                ));
            CreateMap<TimeGroup, AssignmentTimeGroupDto>();
            CreateMap<PatrolRoute, PatrolRouteDto>();


            CreateMap<PatrolAssignmentRM, PatrolAssignmentDto>();
        }
    }
    
    public class PatrolCaseProfile : Profile
    {
        public PatrolCaseProfile()
        {
            // Entity/RM -> DTO
            CreateMap<PatrolCaseRM, PatrolCaseDto>();

            // Create DTO -> Entity
            CreateMap<PatrolCaseCreateDto, PatrolCase>()
                .ForMember(dest => dest.PatrolCaseAttachments, opt => opt.Ignore());

            // Update DTO -> Entity
            CreateMap<PatrolCaseUpdateDto, PatrolCase>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Manual Create DTO -> Entity
            CreateMap<PatrolCaseCreateManualDto, PatrolCase>();
            CreateMap<MstSecurityLookUpRM, MstSecurityLookUpDto>();
            CreateMap<PatrolAssignmentLookUpRM, PatrolAssignmentLookUpDto>();
            CreateMap<PatrolRouteMinimalRM, PatrolRouteMinimalDto>();
        }
    }

    public class PatrolCaseAttachmentProfile : Profile
    {
        public PatrolCaseAttachmentProfile()
        {
            CreateMap<PatrolCaseAttachment, PatrolAttachmentDto>();
            
            // Create DTO -> Entity
            CreateMap<PatrolAttachmentCreateDto, PatrolCaseAttachment>();

            // Update DTO -> Entity
            CreateMap<PatrolAttachmentUpdateDto, PatrolCaseAttachment>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}