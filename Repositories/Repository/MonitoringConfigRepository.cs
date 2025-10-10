using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MonitoringConfigRepository : BaseRepository
    {
        public MonitoringConfigRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MonitoringConfig> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MonitoringConfig>> GetAllAsync()
        {
             return await GetAllQueryable().ToListAsync();
        }

        public async Task<MonitoringConfig> AddAsync(MonitoringConfig config)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                config.ApplicationId = applicationId.Value;
            }
            else if (config.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(config.ApplicationId);
            ValidateApplicationIdForEntity(config, applicationId, isSystemAdmin);

            _context.MonitoringConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task UpdateAsync(MonitoringConfig config)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(config.ApplicationId);
            ValidateApplicationIdForEntity(config, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MonitoringConfigs
                .Where(d => d.Id == id);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var config = await query.FirstOrDefaultAsync();

            if (config == null)
                return;

            _context.MonitoringConfigs.Remove(config);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MonitoringConfig> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MonitoringConfigs;

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}
