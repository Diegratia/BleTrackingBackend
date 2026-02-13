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
        private readonly IEmailService _emailService;

        public UserService(
            UserRepository repository,
            UserGroupRepository userGroupRepository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService) : base(httpContextAccessor)
        {
            _repository = repository;
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
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

        public async Task<UserDto> CreateAsync(RegisterDto dto)
        {
            var currentUser = await _repository.GetByIdEntityAsync(UserIdFromToken);
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole == LevelPriority.Primary || currentUserRole == LevelPriority.PrimaryAdmin)
            {
                await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId, LevelPriority.UserCreated, LevelPriority.Primary, LevelPriority.PrimaryAdmin);
            }

            if (await _repository.EmailExistsAsync(dto.Email.ToLower()))
                throw new Exception("Email is already registered");

            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var userGroup = await _userGroupRepository.GetByIdAsync(dto.GroupId);
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                Password = null ?? "",
                IsCreatedPassword = 0,
                IsEmailConfirmation = 0,
                EmailConfirmationCode = confirmationCode,
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.MinValue,
                Status = 1,
                ApplicationId = userGroup.ApplicationId,
                GroupId = dto.GroupId,
                // Permission flags - default null (inherit dari Group)
                CanApprovePatrol = null,
                CanAlarmAction = null
            };

            await _repository.AddAsync(newUser);
            // Send confirmation email
            await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.Username, confirmationCode);

             _audit.Created(
                "User",
                newUser.Id,
                "Created User",
                new { newUser.Username, newUser.Email }
            );

            var result = await _repository.GetByIdAsync(newUser.Id);
            return _mapper.Map<UserDto>(result);
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
