using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class UserBuildingAccessRepository : BaseRepository
    {
        public UserBuildingAccessRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        /// <summary>
        /// Get all building accesses for a specific user
        /// </summary>
        public async Task<IEnumerable<UserBuildingAccess>> GetByUserIdAsync(Guid userId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.UserBuildingAccesses
                .Include(uba => uba.Building)
                .Where(uba => uba.UserId == userId && uba.Status != 0);

            // Apply multi-tenancy filter for non-system admins
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get all users with access to a specific building
        /// </summary>
        public async Task<IEnumerable<UserBuildingAccess>> GetByBuildingIdAsync(Guid buildingId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.UserBuildingAccesses
                .Include(uba => uba.User)
                .Where(uba => uba.BuildingId == buildingId && uba.Status != 0);

            // Apply multi-tenancy filter for non-system admins
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get list of accessible building IDs for a user
        /// Used for filtering in JWT token generation
        /// </summary>
        public async Task<List<Guid>> GetAccessibleBuildingIdsAsync(Guid userId)
        {
            return await _context.UserBuildingAccesses
                .Where(uba => uba.UserId == userId && uba.Status != 0)
                .Select(uba => uba.BuildingId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// Get a specific access record by user and building
        /// </summary>
        public async Task<UserBuildingAccess?> GetByUserAndBuildingAsync(Guid userId, Guid buildingId)
        {
            return await _context.UserBuildingAccesses
                .FirstOrDefaultAsync(uba =>
                    uba.UserId == userId &&
                    uba.BuildingId == buildingId &&
                    uba.Status != 0
                );
        }

        /// <summary>
        /// Add a single building access
        /// </summary>
        public async Task AddAccessAsync(UserBuildingAccess access)
        {
            _context.UserBuildingAccesses.Add(access);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Add multiple building accesses
        /// </summary>
        public async Task AddAccessRangeAsync(IEnumerable<UserBuildingAccess> accesses)
        {
            _context.UserBuildingAccesses.AddRange(accesses);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove building access (soft delete by setting Status = 0)
        /// </summary>
        public async Task RemoveAccessAsync(Guid userId, Guid buildingId)
        {
            var access = await _context.UserBuildingAccesses
                .FirstOrDefaultAsync(uba =>
                    uba.UserId == userId &&
                    uba.BuildingId == buildingId
                );

            if (access != null)
            {
                access.Status = 0;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Remove all building accesses for a user (soft delete)
        /// </summary>
        public async Task RemoveAllAccessesByUserAsync(Guid userId)
        {
            var accesses = await _context.UserBuildingAccesses
                .Where(uba => uba.UserId == userId)
                .ToListAsync();

            foreach (var access in accesses)
            {
                access.Status = 0;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove all user accesses for a building (soft delete)
        /// </summary>
        public async Task RemoveAllAccessesByBuildingAsync(Guid buildingId)
        {
            var accesses = await _context.UserBuildingAccesses
                .Where(uba => uba.BuildingId == buildingId)
                .ToListAsync();

            foreach (var access in accesses)
            {
                access.Status = 0;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Check if user has access to a specific building
        /// </summary>
        public async Task<bool> HasAccessAsync(Guid userId, Guid buildingId)
        {
            return await _context.UserBuildingAccesses
                .AnyAsync(uba =>
                    uba.UserId == userId &&
                    uba.BuildingId == buildingId &&
                    uba.Status != 0
                );
        }
    }
}
