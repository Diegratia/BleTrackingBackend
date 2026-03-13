using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    public class MstApplicationRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstApplicationRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
            _context = context;
        }

        private IQueryable<MstApplication> BaseEntityQuery()
        {
            return _context.MstApplications
                .Where(x => x.ApplicationStatus != 0);
        }

        private IQueryable<MstApplicationRead> ProjectToRead(IQueryable<MstApplication> query)
        {
            return query.AsNoTracking().Select(x => new MstApplicationRead
            {
                Generate = x.Generate,
                Id = x.Id,
                ApplicationName = x.ApplicationName,
                OrganizationType = x.OrganizationType.ToString(),
                OrganizationAddress = x.OrganizationAddress,
                ApplicationType = x.ApplicationType.ToString(),
                ApplicationRegistered = x.ApplicationRegistered,
                ApplicationExpired = x.ApplicationExpired,
                HostName = x.HostName,
                HostPhone = x.HostPhone,
                HostEmail = x.HostEmail,
                HostAddress = x.HostAddress,
                ApplicationCustomName = x.ApplicationCustomName,
                ApplicationCustomDomain = x.ApplicationCustomDomain,
                ApplicationCustomPort = x.ApplicationCustomPort,
                LicenseCode = x.LicenseCode,
                LicenseType = x.LicenseType,
                ApplicationStatus = x.ApplicationStatus ?? 1,
                PatrolTrackingMode = x.PatrolTrackingMode,
                EnabledFeatures = x.EnabledFeatures,
                LicenseTier = x.LicenseTier,
                CustomerName = x.CustomerName
            });
        }

        public async Task<MstApplicationRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery()).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MstApplication?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<MstApplicationRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<MstApplicationRead> Data, int Total, int Filtered)> FilterAsync(MstApplicationFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(x =>
                    (x.ApplicationName != null && x.ApplicationName.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.HostName != null && x.HostName.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.LicenseCode != null && x.LicenseCode.ToLower().Contains(filter.Search.ToLower())));

            if (filter.ApplicationStatus.HasValue)
                query = query.Where(x => x.ApplicationStatus == filter.ApplicationStatus.Value);

            var total = await query.CountAsync();
            var filtered = total;

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<MstApplication> AddAsync(MstApplication application)
        {
            _context.MstApplications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task UpdateAsync(MstApplication application)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MstApplication application)
        {
            application.ApplicationStatus = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<List<MstApplication>> GetAllExportAsync()
        {
            return await BaseEntityQuery().ToListAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
