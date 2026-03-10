using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class UserService : BaseService, IUserService
    {
        private readonly UserRepository _repository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public UserService(
            UserRepository repository,
            UserGroupRepository userGroupRepository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserRead> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException($"User with id {id} not found");
            return user;
        }

        public async Task<UserRead?> GetFromTokenAsync()
        {
            var userId = UserIdFromToken;
            return await _repository.GetByIdAsync(userId);
        }

        public async Task<IEnumerable<UserRead>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return users;
        }

        public async Task<IEnumerable<UserRead>> GetAllIntegrationAsync()
        {
            var users = await _repository.GetAllIntegrationAsync();
            return _mapper.Map<IEnumerable<UserRead>>(users);
        }

        public async Task<UserRead> CreateAsync(CreateUserDto dto)
        {
            var currentUser = await _repository.GetByIdEntityAsync(UserIdFromToken);
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole == LevelPriority.Primary || currentUserRole == LevelPriority.PrimaryAdmin)
            {
                await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId,
                    LevelPriority.UserCreated, LevelPriority.Primary, LevelPriority.PrimaryAdmin);
            }

            if (await _repository.EmailExistsAsync(dto.Email.ToLower()))
                throw new Exception("Email is already registered");

            if (string.IsNullOrEmpty(dto.Password))
                throw new ArgumentException("Password is required for direct user creation");

            var userGroup = await _userGroupRepository.GetByIdAsync(dto.GroupId);
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Hash password
                IsCreatedPassword = 1,  // Password already set
                IsEmailConfirmation = 1, // Auto-confirmed
                EmailConfirmationCode = "direct",
                EmailConfirmationExpiredAt = DateTime.UtcNow, // Set to current time since already confirmed
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.MinValue,
                Status = 1, // Active
                ApplicationId = userGroup.ApplicationId,
                GroupId = dto.GroupId,
                // Permission flags dari DTO (bukan hardcoded null)
                CanApprovePatrol = dto.CanApprovePatrol,
                CanAlarmAction = dto.CanAlarmAction,
                CanCreateMonitoringConfig = dto.CanCreateMonitoringConfig,
                CanUpdateMonitoringConfig = dto.CanUpdateMonitoringConfig
            };

            await _repository.AddAsync(newUser);

            _audit.Created(
                "User",
                newUser.Id,
                "Created User (Direct - No Email Verification)",
                new { newUser.Username, newUser.Email }
            );

            var result = await _repository.GetByIdAsync(newUser.Id);
            return result;  // Direct return pattern - repository already returns UserRead
        }

        public async Task UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var currentUser = await _repository.GetByIdEntityAsync(UserIdFromToken);
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var user = await _repository.GetByIdEntityAsync(id);
            if (user == null)
                throw new NotFoundException($"User with id {id} not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole == LevelPriority.Primary || currentUserRole == LevelPriority.PrimaryAdmin)
            {
                await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId, LevelPriority.UserCreated, LevelPriority.Primary, LevelPriority.PrimaryAdmin);
            }

            user.Username = dto.Username.ToLower();
            user.Email = dto.Email.ToLower();
            user.GroupId = dto.GroupId;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                user.IsCreatedPassword = 1;
            }

            // Permission override flags (nullable = inherit dari Group)
            if (dto.CanApprovePatrol.HasValue)
                user.CanApprovePatrol = dto.CanApprovePatrol.Value;
            if (dto.CanAlarmAction.HasValue)
                user.CanAlarmAction = dto.CanAlarmAction.Value;
            if (dto.CanCreateMonitoringConfig.HasValue)
                user.CanCreateMonitoringConfig = dto.CanCreateMonitoringConfig.Value;
            if (dto.CanUpdateMonitoringConfig.HasValue)
                user.CanUpdateMonitoringConfig = dto.CanUpdateMonitoringConfig.Value;

            await _repository.UpdateAsync(user);

             _audit.Updated(
                "User",
                user.Id,
                "Updated User",
                new { user.Username, user.Email }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var currentUser = await _repository.GetByIdEntityAsync(UserIdFromToken);
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.System && currentUserRole != LevelPriority.SuperAdmin && currentUserRole != LevelPriority.PrimaryAdmin)
                throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can delete user");

            var user = await _repository.GetByIdEntityAsync(id);
            if (user == null)
                throw new NotFoundException($"User with id {id} not found");

            
            await _repository.SoftDeleteAsync(id);

             _audit.Deleted(
                "User",
                user.Id,
                "Deleted User",
                new { user.Username, user.Email }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, UserFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // Map Date Filters (Generic Dictionary -> Specific Prop)
            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }
    }
}
