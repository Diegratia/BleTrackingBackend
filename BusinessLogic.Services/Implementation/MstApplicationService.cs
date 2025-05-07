using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstApplicationService : IMstApplicationService
    {
        private readonly MstApplicationRepository _applicationRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;

        public MstApplicationService(
            MstApplicationRepository applicationRepository,
            UserGroupRepository userGroupRepository,
            UserRepository userRepository,
            IMapper mapper)
        {
            _applicationRepository = applicationRepository;
            _userGroupRepository = userGroupRepository;
            _userRepository = userRepository;
            _mapper = mapper;
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
            application.Id = Guid.NewGuid();
            application.ApplicationStatus = 1;

            var createdApplication = await _applicationRepository.AddAsync(application);

            var userGroup = await _userGroupRepository.GetByApplicationIdAndPriorityAsync(
                createdApplication.Id, LevelPriority.Primary);

            if (userGroup == null)
            {
                userGroup = new UserGroup
                {
                    Id = Guid.NewGuid(),
                    Name = $"Primary Group for {createdApplication.Id}",
                    LevelPriority = LevelPriority.Primary,
                    ApplicationId = createdApplication.Id,
                    CreatedBy = "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedAt = DateTime.UtcNow,
                    Status = 1
                };
                await _userGroupRepository.AddAsync(userGroup);
            }

            var primaryUser = new User
            {
                Id = Guid.NewGuid(),
                Username = $"primary_{createdApplication.Id}",
                Password = "hashed_default_password",
                IsCreatedPassword = 0,
                Email = $"primary_{createdApplication.Id}@example.com",
                IsEmailConfirmation = 0,
                EmailConfirmationCode = Guid.NewGuid().ToString(),
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                StatusActive = StatusActive.Active,
                GroupId = userGroup.Id
            };

            await _userRepository.AddAsync(primaryUser);

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