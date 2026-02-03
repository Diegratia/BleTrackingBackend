using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.ViewModels;
using Entities.Models;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Extension
{
    public class MstDepartmentProfile : Profile
    {
        public MstDepartmentProfile()
        {
            CreateMap<MstDepartment, MstDepartmentRead>();
            CreateMap<MstDepartment, MstDepartmentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<MstDepartment, OpenMstDepartmentDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<MstDepartmentCreateDto, MstDepartment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
            CreateMap<MstDepartmentUpdateDto, MstDepartment>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Generate, opt => opt.Ignore());
        }

    }
}
