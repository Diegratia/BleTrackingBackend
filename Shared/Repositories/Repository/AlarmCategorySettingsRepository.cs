using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class AlarmCategorySettingsRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public AlarmCategorySettingsRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
            _context = context;
        }

        private IQueryable<AlarmCategorySettings> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.AlarmCategorySettings;
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<AlarmCategorySettingsRead> ProjectToRead(IQueryable<AlarmCategorySettings> query)
        {
            return query.AsNoTracking().Select(x => new AlarmCategorySettingsRead
            {
                Id = x.Id,
                AlarmCategory = x.AlarmCategory.ToString(),
                Remarks = x.Remarks,
                AlarmColor = x.AlarmColor,
                AlarmLevelPriority = x.AlarmLevelPriority.ToString(),
                NotifyIntervalSec = x.NotifyIntervalSec,
                IsEnabled = x.IsEnabled
            });
        }

        public async Task<AlarmCategorySettingsRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery()).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<AlarmCategorySettings?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<AlarmCategorySettingsRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<AlarmCategorySettingsRead> Data, int Total, int Filtered)> FilterAsync(AlarmCategorySettingsFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(x =>
                    (x.Remarks != null && x.Remarks.ToLower().Contains(filter.Search.ToLower())));

            if (filter.IsEnabled.HasValue)
                query = query.Where(x => x.IsEnabled == filter.IsEnabled.Value);

            var total = await query.CountAsync();
            var filtered = total;

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<AlarmCategorySettings> AddAsync(AlarmCategorySettings category)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");
                category.ApplicationId = applicationId.Value;
            }
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
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AlarmCategorySettings category)
        {
            _context.AlarmCategorySettings.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AlarmCategorySettings>> GetAllExportAsync()
        {
            return await BaseEntityQuery().ToListAsync();
        }
    }
}
