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

        public async Task<UserGroupRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<UserGroup?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UserGroupRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public IQueryable<UserGroup> GetAllQueryable()
        {
            return BaseEntityQuery();
        }

        private IQueryable<UserGroupRead> ProjectToRead(IQueryable<UserGroup> query)
        {
            return query.AsNoTracking().Select(x => new UserGroupRead
            {
                Id = x.Id,
                Name = x.Name,
                LevelPriority = x.LevelPriority.ToString(),
                ApplicationId = x.ApplicationId,
            });
        }

        public async Task<(List<UserGroupRead> Data, int Total, int Filtered)> FilterAsync(
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

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();
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
    }
}
