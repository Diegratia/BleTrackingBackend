using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Data.ViewModels;

namespace BusinessLogic.Services.Extension
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<User, AuthResponseDto>();

            CreateMap<UserGroup, UserGroupDto>()
                .ForMember(dest => dest.LevelPriority, opt => opt.MapFrom(src => src.LevelPriority.ToString()));

            CreateMap<CreateUserGroupDto, UserGroup>()
                .ForMember(dest => dest.LevelPriority, opt => opt.MapFrom(src => src.LevelPriority.ToString()));

            CreateMap<User, UserDto>();
        }
    }
}