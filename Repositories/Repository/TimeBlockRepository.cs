
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
    public class TimeBlockRepository : BaseRepository
    {
        public TimeBlockRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<TimeBlock> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeBlocks
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TimeBlock>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeBlocks
                .Where(b => b.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }

        public async Task<TimeBlock> AddAsync(TimeBlock entity)
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

            _context.TimeBlocks.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TimeBlock> RawAddAsync(TimeBlock entity)
        {
            _context.TimeBlocks.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TimeBlock entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            // _context.MstBrands.Update(brand); // Optional
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeBlocks
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var entity = await query.FirstOrDefaultAsync();

            if (entity == null)
                throw new KeyNotFoundException("TimeBlock not found");

            entity.Status = 0;
            await _context.SaveChangesAsync();
        }

        public IQueryable<TimeBlock> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeBlocks
                .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<TimeBlock>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeBlocks
                .Where(d => d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }
    }
}
