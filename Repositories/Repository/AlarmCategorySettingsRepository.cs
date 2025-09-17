using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Repositories.Repository
{
    public class AlarmCategorySettingsRepository : BaseRepository
    {
        public AlarmCategorySettingsRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<AlarmCategorySettings> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Category not found");
        }
        public async Task<AlarmCategorySettings> AddAsync(AlarmCategorySettings category)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

                // non system ambil dari claim
                if (!isSystemAdmin)
                {
                    if (!applicationId.HasValue)
                        throw new UnauthorizedAccessException("ApplicationId not found in context");
                    category.ApplicationId = applicationId.Value;
                }
                // admin set applciation di body
                else if (category.ApplicationId == Guid.Empty)
                {
                    throw new ArgumentException("System admin must provide a valid ApplicationId");
                }
            await ValidateApplicationIdAsync(category.ApplicationId);
            ValidateApplicationIdForEntity(category, applicationId, isSystemAdmin);
            
            _context.AlarmCategorySettings.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(AlarmCategorySettings category)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            await ValidateApplicationIdAsync(category.ApplicationId);
            ValidateApplicationIdForEntity(category, applicationId, isSystemAdmin);

            // _context.MstDistricts.Update(district);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.AlarmCategorySettings
                .Where(d => d.Id == id );
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var category = await query.FirstOrDefaultAsync();
            if (category == null)
                throw new KeyNotFoundException("category not found");

            _context.AlarmCategorySettings.Remove(category);
            await _context.SaveChangesAsync();
        }

          public async Task<IEnumerable<AlarmCategorySettings>> GetAllAsync()
        {
            return await GetAllQueryable()
                .ToListAsync();
        }

        public IQueryable<AlarmCategorySettings> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.AlarmCategorySettings;

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<AlarmCategorySettings>> GetAllExportAsync()
        {
            return await GetAllQueryable()
                .ToListAsync();
        }
    }
}







