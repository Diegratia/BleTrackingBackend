
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
    public class TimeGroupRepository : BaseRepository
    {
        public TimeGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

            public async Task<TimeGroup?> GetByIdAsync(Guid id)
            {
                return await GetAllQueryable()
                    .Where(tg => tg.Id == id && tg.Status != 0)
                    .FirstOrDefaultAsync();
            }

            public async Task<IEnumerable<TimeGroup>> GetAllAsync()
            {
                return await GetAllQueryable()
                .ToListAsync();
            }

        public async Task<TimeGroup> AddAsync(TimeGroup entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                entity.ApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            _context.TimeGroups.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TimeGroup entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            // _context.TimeGroups.Update(entity); // Optional
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Include(d => d.TimeBlocks)
                .Include(ca => ca.CardAccessTimeGroups)
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var entity = await query.FirstOrDefaultAsync();
            if (entity == null)
                throw new KeyNotFoundException("TimeGroup not found");
                
            await _context.SaveChangesAsync();
        }

        public IQueryable<TimeGroup> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Include(d => d.TimeBlocks)
                .Include(ca => ca.CardAccessTimeGroups)
                .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<TimeGroup>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Where(d => d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }
    }
}
