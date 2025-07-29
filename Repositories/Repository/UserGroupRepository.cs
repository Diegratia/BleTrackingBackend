using System;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class UserGroupRepository : BaseRepository
    {
        public UserGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<UserGroup?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.UserGroups
                .Where(g => g.Id == id && g.Status != 0);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<UserGroup?> GetByApplicationIdAndPriorityAsync(Guid applicationId, LevelPriority levelPriority)
        {
            var (userAppId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && userAppId != applicationId)
                throw new UnauthorizedAccessException("You are not allowed to access groups from another application.");

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

            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();
            return userGroup;
        }

        // public async Task ValidateGroupRoleAsync(Guid groupId, LevelPriority requiredPriority)
        // {
        //     var group = await GetByIdAsync(groupId);
        //     if (group == null)
        //         throw new ArgumentException($"Group with ID {groupId} not found.");
        //     if (group.LevelPriority != requiredPriority)
        //         throw new UnauthorizedAccessException($"Group must have {requiredPriority} role.");
        // }

                public async Task ValidateGroupRoleAsync(Guid groupId, params LevelPriority[] allowedPriorities)
            {
                var group = await GetByIdAsync(groupId);
                if (group == null)
                    throw new ArgumentException($"Group with ID {groupId} not found.");
                if (!allowedPriorities.Contains(group.LevelPriority))
                    throw new UnauthorizedAccessException($"Group must have one of the allowed roles: {string.Join(", ", allowedPriorities)}.");
            }

    }
}




// using System;
// using System.Threading.Tasks;
// using Entities.Models;
// using Microsoft.EntityFrameworkCore;
// using Repositories.DbContexts;

// namespace Repositories.Repository
// {
//     public class UserGroupRepository
//     {
//         private readonly BleTrackingDbContext _context;

//         public UserGroupRepository(BleTrackingDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<UserGroup> GetByIdAsync(Guid id)
//         {
//             return await _context.UserGroups
//                 .FirstOrDefaultAsync(g => g.Id == id && g.Status != 0);
//         }

//         public async Task<UserGroup?> GetByIdAsync(Guid id)
//         {
//             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

//             var query = _context.UserGroups
//                 .Where(g => g.Id == id && g.Status != 0);

//             return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
//         }

//         public async Task<UserGroup> GetByApplicationIdAndPriorityAsync(Guid applicationId, LevelPriority levelPriority)
//         {
//             return await _context.UserGroups
//                 .FirstOrDefaultAsync(ug => ug.ApplicationId == applicationId && ug.LevelPriority == levelPriority && ug.Status != 0);
//         }

//         public async Task<UserGroup> AddAsync(UserGroup userGroup)
//         {
//             _context.UserGroups.AddRange(userGroup);
//             await _context.SaveChangesAsync();
//             return userGroup;
//         }

//         public async Task ValidateGroupRoleAsync(Guid groupId, LevelPriority requiredPriority)
//         {
//             var group = await GetByIdAsync(groupId);
//             if (group == null)
//                 throw new ArgumentException($"Group with ID {groupId} not found.");
//             if (group.LevelPriority != requiredPriority)
//                 throw new UnauthorizedAccessException($"Group must have {requiredPriority} role.");
//         }
//     }
// }







