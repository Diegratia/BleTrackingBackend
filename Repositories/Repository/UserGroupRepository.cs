using System;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class UserGroupRepository
    {
        private readonly BleTrackingDbContext _context;

        public UserGroupRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<UserGroup> GetByIdAsync(Guid id)
        {
            return await _context.UserGroups
                .FirstOrDefaultAsync(g => g.Id == id && g.Status != 0);
        }

        public async Task<UserGroup> GetByApplicationIdAndPriorityAsync(Guid applicationId, LevelPriority levelPriority)
        {
            return await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.ApplicationId == applicationId && ug.LevelPriority == levelPriority && ug.Status != 0);
        }

        public async Task<UserGroup> AddAsync(UserGroup userGroup)
        {
            _context.UserGroups.AddRange(userGroup);
            await _context.SaveChangesAsync();
            return userGroup;
        }

        public async Task ValidateGroupRoleAsync(Guid groupId, LevelPriority requiredPriority)
        {
            var group = await GetByIdAsync(groupId);
            if (group == null)
                throw new ArgumentException($"Group with ID {groupId} not found.");
            if (group.LevelPriority != requiredPriority)
                throw new UnauthorizedAccessException($"Group must have {requiredPriority} role.");
        }
    }
}