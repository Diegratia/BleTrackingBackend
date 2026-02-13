using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class UserGroupRepository : BaseRepository
    {
        public UserGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<UserGroup> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.UserGroups
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<UserGroupWithDetailsRead?> GetByIdAsync(Guid id)
        {
            return await GetByGroupIdWithDetailsAsync(id);
        }

        public async Task<UserGroup?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserGroupWithDetailsRead>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = BaseEntityQuery();
            var groups = await query.ToListAsync();

            var result = new List<UserGroupWithDetailsRead>();
            foreach (var group in groups)
            {
                var groupRead = await GetByGroupIdWithDetailsAsync(group.Id);
                if (groupRead != null)
                    result.Add(groupRead);
            }

            return result;
        }

        public IQueryable<UserGroup> GetAllQueryable()
        {
            return BaseEntityQuery();
        }

        public async Task<(List<UserGroupWithDetailsRead> Data, int Total, int Filtered)> FilterAsync(
            UserGroupFilter filter)
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(filter.LevelPriority))
            {
                query = query.Where(x => x.LevelPriority.ToString() == filter.LevelPriority);
            }

            var applicationIds = ExtractIds(filter.ApplicationId);
            if (applicationIds.Count > 0)
                query = query.Where(x => applicationIds.Contains(x.ApplicationId));

            // Filter by member IDs - groups that contain these specific members
            var memberIds = ExtractIds(filter.MemberId);
            if (memberIds.Count > 0)
            {
                query = query.Where(ug =>
                    _context.Users
                        .Any(u => u.GroupId == ug.Id
                            && u.Status != 0
                            && memberIds.Contains(u.Id))
                );
            }

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var groupIds = await query.Select(x => x.Id).ToListAsync();
            var data = new List<UserGroupWithDetailsRead>();

            // Sequential foreach loop (DbContext is not thread-safe)
            foreach (var groupId in groupIds)
            {
                var groupRead = await GetByGroupIdWithDetailsAsync(groupId);
                if (groupRead != null)
                    data.Add(groupRead);
            }

            return (data, total, filtered);
        }

        public async Task<UserGroup?> GetByApplicationIdAndPriorityAsync(Guid applicationId, LevelPriority levelPriority)
        {
            var (userAppId, isSystemAdmin) = GetApplicationIdAndRole();
            if (!isSystemAdmin && userAppId != applicationId)
                throw new UnauthorizedAccessException("You are not allowed to access groups from another application.");

            return await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.ApplicationId == applicationId && ug.LevelPriority == levelPriority && ug.Status != 0);
        }

        public async Task<UserGroup?> GetByApplicationIdAndPriorityAsyncRaw(Guid applicationId, LevelPriority levelPriority)
        {
            return await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.ApplicationId == applicationId && ug.LevelPriority == levelPriority && ug.Status != 0);
        }

        public async Task<UserGroup> AddAsync(UserGroup userGroup)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId is required for non-admin.");
                userGroup.ApplicationId = applicationId.Value;
            }
            else if (userGroup.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must explicitly define ApplicationId.");
            }

            await ValidateApplicationIdAsync(userGroup.ApplicationId);
            ValidateApplicationIdForEntity(userGroup, applicationId, isSystemAdmin);

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();
            return userGroup;
        }

        public async Task<UserGroup> AddAsyncRaw(UserGroup userGroup)
        {
            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();
            return userGroup;
        }

        public async Task UpdateAsync(UserGroup userGroup)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(userGroup.ApplicationId);
            ValidateApplicationIdForEntity(userGroup, applicationId, isSystemAdmin);

            _context.Entry(userGroup).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var group = await GetByIdEntityAsync(id);
            if (group == null)
                throw new KeyNotFoundException("User group not found");

            // Check if group still has active users
            var hasUsers = await _context.Users.AnyAsync(u => u.GroupId == id && u.Status != 0);
            if (hasUsers)
                throw new InvalidOperationException("Cannot delete group with active users");

            group.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var group = await GetByIdEntityAsync(id);
            if (group == null)
                throw new KeyNotFoundException("User group not found");

            // Check if group still has active users
            var hasUsers = await _context.Users.AnyAsync(u => u.GroupId == id && u.Status != 0);
            if (hasUsers)
                throw new InvalidOperationException("Cannot delete group with active users");

            group.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task ValidateGroupRoleAsync(Guid groupId, params LevelPriority[] allowedPriorities)
        {
            var group = await GetByIdEntityAsync(groupId);
            if (group == null)
                throw new ArgumentException($"Group with ID {groupId} not found.");
            if (!allowedPriorities.Any(ap => ap == group.LevelPriority))
                throw new UnauthorizedAccessException($"Group must have one of the allowed roles: {string.Join(", ", allowedPriorities)}.");
        }

        public async Task<UserGroupWithDetailsRead?> GetByGroupIdWithDetailsAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // Get group entity
            var group = await BaseEntityQuery().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (group == null)
                return null;

            // Get accessible buildings for this group
            var buildingQuery = _context.GroupBuildingAccesses
                .Where(gba => gba.GroupId == id && gba.Status != 0);

            // Apply multi-tenancy filter for buildings
            if (!isSystemAdmin && applicationId.HasValue)
            {
                buildingQuery = buildingQuery.Where(gba => gba.ApplicationId == applicationId.Value);
            }

            var buildings = await buildingQuery
                .Select(gba => new MstBuildingRead
                {
                    Id = gba.BuildingId,
                    Name = gba.Building != null ? gba.Building.Name : "",
                    ApplicationId = gba.ApplicationId,
                    Status = gba.Status,
                    CreatedAt = gba.CreatedAt,
                    UpdatedAt = gba.UpdatedAt
                })
                .ToListAsync();

            // Get members of this group
            var usersQuery = _context.Users
                .Where(u => u.GroupId == id && u.Status != 0);

            // Apply multi-tenancy filter for users
            if (!isSystemAdmin && applicationId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.ApplicationId == applicationId.Value);
            }

            var membersList = await usersQuery
                .Select(u => new UserGroupMemberRead
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.Username,
                    Status = u.Status
                })
                .ToListAsync();

            // Combine all data
            return new UserGroupWithDetailsRead
            {
                Id = group.Id,
                Name = group.Name,
                LevelPriority = group.LevelPriority.ToString(),
                ApplicationId = group.ApplicationId,
                IsHead = group.IsHead,
                AccessibleBuildings = buildings,
                Members = membersList,
                MemberCount = membersList.Count,
                AccessibleBuildingCount = buildings.Count,
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                CreatedBy = group.CreatedBy,
                UpdatedBy = group.UpdatedBy
            };
        }
    }
}
