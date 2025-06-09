using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Services.Implementation
{
    public class MstApplicationService : IMstApplicationService
    {
        private readonly MstApplicationRepository _applicationRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstApplicationService(
            MstApplicationRepository applicationRepository,
            UserGroupRepository userGroupRepository,
            UserRepository userRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationRepository = applicationRepository;
            _userGroupRepository = userGroupRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<MstApplicationDto>> GetAllApplicationsAsync()
        {
            var applications = await _applicationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstApplicationDto>>(applications);
        }

        public async Task<MstApplicationDto> GetApplicationByIdAsync(Guid id)
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            return application == null ? null : _mapper.Map<MstApplicationDto>(application);
        }

        public async Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = _mapper.Map<MstApplication>(dto);
            // application.Id = Guid.NewGuid();
            application.ApplicationStatus = 1;

            var createdApplication = await _applicationRepository.AddAsync(application);

            // var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(
            //     createdApplication.Id, LevelPriority.Primary);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            // if (userGroup == null)
            // {
            //     userGroup = new UserGroup
            //     {
            //         Id = Guid.NewGuid(),
            //         Name = $"Primary Group for {createdApplication.Id}",
            //         LevelPriority = LevelPriority.Primary,
            //         ApplicationId = createdApplication.Id,
            //         CreatedBy = username,
            //         CreatedAt = DateTime.UtcNow,
            //         UpdatedBy = username,
            //         UpdatedAt = DateTime.UtcNow,
            //         Status = 1
            //     };
            //     await _userGroupRepository.AddAsync(userGroup);
            // }

             var userGroups = new List<UserGroup>
            {
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = application.Id,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Operator",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = application.Id,
                     CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Security",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = application.Id,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                },
                new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "Other Primary",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = application.Id,
                    CreatedBy = username,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = username,
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                }
            };

            foreach(var group in userGroups)
            {
                await _userGroupRepository.AddAsync(group);
            }
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser1",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1, 
                    Email = "testprimaryuser1@example.com", 
                    IsEmailConfirmation = 0, 
                    EmailConfirmationCode = Guid.NewGuid().ToString(), 
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1), 
                    EmailConfirmationAt = DateTime.UtcNow, 
                    LastLoginAt = DateTime.UtcNow, 
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[0].Id // Admin
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser2",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser2@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1), 
                    EmailConfirmationAt = DateTime.UtcNow, 
                    LastLoginAt = DateTime.UtcNow, 
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[1].Id // Operator
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser3",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser3@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1), 
                    EmailConfirmationAt = DateTime.UtcNow, 
                    LastLoginAt = DateTime.UtcNow, 
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[2].Id // Security
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Username = "TestPrimaryUser4",
                    Password = BCrypt.Net.BCrypt.HashPassword("testprimaryuser123@"),
                    IsCreatedPassword = 1,
                    Email = "testprimaryuser4@example.com",
                    IsEmailConfirmation = 0,
                    EmailConfirmationCode = Guid.NewGuid().ToString(),
                    EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(1), 
                    EmailConfirmationAt = DateTime.UtcNow, 
                    LastLoginAt = DateTime.UtcNow, 
                    StatusActive = StatusActive.Active,
                    GroupId = userGroups[3].Id // other primary
                }
            };

            foreach (var user in users)
            {
                await _userRepository.AddAsync(user);
            }

            // await _applicationRepository.AddAsync(application);

            return _mapper.Map<MstApplicationDto>(createdApplication);
        }

        public async Task UpdateApplicationAsync(Guid id, MstApplicationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var application = await _applicationRepository.GetByIdAsync(id);
            if (application == null)
                throw new KeyNotFoundException($"Application with ID {id} not found");

            _mapper.Map(dto, application);

            await _applicationRepository.UpdateAsync(application);
        }

        public async Task DeleteApplicationAsync(Guid id)
        {
            await _applicationRepository.DeleteAsync(id);
        }
    }
}