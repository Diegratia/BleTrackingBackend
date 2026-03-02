
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class MstDistrictRepository : BaseRepository
    {
        public MstDistrictRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstDistrict?> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(d => d.Id == id && d.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<MstFloorplan> GetFloorplanByIdAsync(Guid floorplanId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            if (!isSystemAdmin && applicationId == null)
                throw new UnauthorizedAccessException("ApplicationId not found in context");

            var query = _context.MstFloorplans
                .Where(f => f.Id == floorplanId && f.Status != 0);
            query = query.WithActiveRelations();
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<MstDistrict> AddAsync(MstDistrict district)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // non system ambil dari claim
            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");
                district.ApplicationId = applicationId.Value;
            }
            // admin set applciation di body
            else if (district.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }
            await ValidateApplicationIdAsync(district.ApplicationId);
            ValidateApplicationIdForEntity(district, applicationId, isSystemAdmin);

            _context.MstDistricts.Add(district);
            await _context.SaveChangesAsync();
            return district;
        }

        public async Task UpdateAsync(MstDistrict district)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            await ValidateApplicationIdAsync(district.ApplicationId);
            ValidateApplicationIdForEntity(district, applicationId, isSystemAdmin);

            // _context.MstDistricts.Update(district);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstDistricts
                .Where(d => d.Id == id && d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var district = await query.FirstOrDefaultAsync();
            if (district == null)
                throw new KeyNotFoundException("District not found");

            district.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MstDistrictRead>> GetAllAsync()
        {
            return await ProjectToRead(GetAllQueryable())
                .ToListAsync();
        }

        public IQueryable<MstDistrict> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstDistricts
                .Where(d => d.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstDistrictRead>> GetAllExportAsync()
        {
            return await ProjectToRead(GetAllQueryable())
                .ToListAsync();
        }
        public async Task<(List<MstDistrictRead> Data, int Total, int Filtered)> FilterAsync(MstDistrictFilter filter)
        {
            var query = GetAllQueryable();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search) || x.Code.ToLower().Contains(search));
            }

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        private IQueryable<MstDistrictRead> ProjectToRead(IQueryable<MstDistrict> query)
        {
            return query.AsNoTracking().Select(x => new MstDistrictRead
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DistrictHost = x.DistrictHost,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt,
                ApplicationId = x.ApplicationId
            });
        }
    }
}











