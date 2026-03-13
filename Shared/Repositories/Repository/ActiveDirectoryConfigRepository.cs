using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    /// <summary>
    /// Repository for Active Directory configuration management
    /// </summary>
    public class ActiveDirectoryConfigRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public ActiveDirectoryConfigRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
            _context = context;
        }

        private IQueryable<ActiveDirectoryConfig> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.ActiveDirectoryConfigs
                .Include(x => x.Application)
                .Include(x => x.DefaultDepartment)
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<ActiveDirectoryConfigRead> ProjectToRead(IQueryable<ActiveDirectoryConfig> query)
        {
            return query.AsNoTracking().Select(x => new ActiveDirectoryConfigRead
            {
                Id = x.Id,
                ApplicationId = x.ApplicationId,
                Server = x.Server,
                Port = x.Port,
                UseSsl = x.UseSsl,
                Domain = x.Domain,
                ServiceAccountDn = x.ServiceAccountDn,
                SearchBase = x.SearchBase,
                UserObjectFilter = x.UserObjectFilter,
                SyncIntervalMinutes = x.SyncIntervalMinutes,
                LastSyncAt = x.LastSyncAt,
                LastSyncStatus = x.LastSyncStatus,
                LastSyncMessage = x.LastSyncMessage,
                TotalUsersSynced = x.TotalUsersSynced,
                IsEnabled = x.IsEnabled,
                AutoCreateMembers = x.AutoCreateMembers,
                DefaultDepartmentId = x.DefaultDepartmentId,
                DefaultDepartmentName = x.DefaultDepartment != null ? x.DefaultDepartment.Name : null,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            });
        }

        public async Task<ActiveDirectoryConfigRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery()).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ActiveDirectoryConfigRead?> GetByApplicationIdAsync(Guid applicationId)
        {
            var (currentAppId, isSystemAdmin) = GetApplicationIdAndRole();

            // If system admin, allow querying any application
            var query = isSystemAdmin
                ? BaseEntityQuery().Where(x => x.ApplicationId == applicationId)
                : BaseEntityQuery().Where(x => x.ApplicationId == currentAppId);

            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<ActiveDirectoryConfig?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<ActiveDirectoryConfigRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<ActiveDirectoryConfig> AddAsync(ActiveDirectoryConfig config)
        {
            _context.ActiveDirectoryConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task UpdateAsync(ActiveDirectoryConfig config)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ActiveDirectoryConfig config)
        {
            config.Status = 0;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update sync status after a sync attempt
        /// </summary>
        public async Task UpdateSyncStatusAsync(Guid configId, DateTime syncTime, string status, string message, int usersSynced)
        {
            var config = await _context.ActiveDirectoryConfigs.FindAsync(configId);
            if (config != null)
            {
                config.LastSyncAt = syncTime;
                config.LastSyncStatus = status;
                config.LastSyncMessage = message;
                config.TotalUsersSynced = usersSynced;
                await _context.SaveChangesAsync();
            }
        }
    }
}
