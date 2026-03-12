using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserGroupService : BaseService, IUserGroupService
    {
        private readonly UserGroupRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserGroupService(
            UserGroupRepository repository,
            UserRepository userRepository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repository = repository;
            _userRepository = userRepository;
            _mapper = mapper;
            _audit = audit;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserGroupWithDetailsRead> GetByIdAsync(Guid id)
        {
            var group = await _repository.GetByIdAsync(id);
            if (group == null)
                throw new NotFoundException($"UserGroup with id {id} not found");
            return group;
        }

        public async Task<IEnumerable<UserGroupWithDetailsRead>> GetAllAsync()
        {
            var groups = await _repository.GetAllAsync();
            return groups;
        }

        public async Task<UserGroupWithDetailsRead> CreateAsync(CreateUserGroupDto dto)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdEntityAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.System && currentUserRole != LevelPriority.SuperAdmin && currentUserRole != LevelPriority.PrimaryAdmin)
                throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can create groups");

            if (!Enum.TryParse<LevelPriority>(dto.LevelPriority!.ToString(), out var targetPriority))
                throw new ArgumentException("Invalid level priority");

            if ((int)targetPriority < (int)currentUserRole)
                throw new UnauthorizedAccessException("You can only assign roles equal to or lower than your own");

            var userGroup = _mapper.Map<UserGroup>(dto);
            userGroup.Id = Guid.NewGuid();
            userGroup.Status = 1;
            userGroup.CreatedBy = UsernameFormToken;
            userGroup.CreatedAt = DateTime.UtcNow;
            userGroup.UpdatedBy = UsernameFormToken;
            userGroup.UpdatedAt = DateTime.UtcNow;
            // IsHead default to false (Operator Biasa)
            userGroup.IsHead = dto.IsHead;

            await _repository.AddAsync(userGroup);

             _audit.Created(
                "UserGroup",
                userGroup.Id,
                "Created UserGroup",
                new { userGroup.Name }
            );

            var result = await _repository.GetByIdAsync(userGroup.Id);
            return result!;
        }

        public async Task UpdateAsync(Guid id, UpdateUserGroupDto dto)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdEntityAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.Primary && currentUserRole != LevelPriority.PrimaryAdmin && currentUserRole != LevelPriority.SuperAdmin && currentUserRole != LevelPriority.System)
                throw  new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can Update groups");

            if (!Enum.TryParse<LevelPriority>(dto.LevelPriority!.ToString(), out var targetPriority))
                throw new ArgumentException("Invalid level priority");

            if ((int)targetPriority < (int)currentUserRole)
                throw new UnauthorizedAccessException("You can only assign roles equal to or lower than your own");

            var userGroup = await _repository.GetByIdEntityAsync(id);
            if (userGroup == null)
                throw new NotFoundException($"UserGroup with id {id} not found");

            userGroup.UpdatedAt = DateTime.UtcNow;
            userGroup.UpdatedBy = UsernameFormToken;

            // Update IsHead if provided
            if (dto.IsHead.HasValue)
                userGroup.IsHead = dto.IsHead.Value;

            _mapper.Map(dto, userGroup);
            await _repository.UpdateAsync(userGroup);

             _audit.Updated(
                "UserGroup",
                userGroup.Id,
                "Updated UserGroup",
                new { userGroup.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdEntityAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.Primary && currentUserRole != LevelPriority.PrimaryAdmin && currentUserRole != LevelPriority.System)
                throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can Delete groups");

            var userGroup = await _repository.GetByIdEntityAsync(id);
            if (userGroup == null)
                throw new NotFoundException($"UserGroup with id {id} not found");

            userGroup.UpdatedAt = DateTime.UtcNow;
            userGroup.UpdatedBy = UsernameFormToken;
            await _repository.SoftDeleteAsync(id);

             _audit.Deleted(
                "UserGroup",
                userGroup.Id,
                "Deleted UserGroup",
                new { userGroup.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, UserGroupFilter filter)
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
