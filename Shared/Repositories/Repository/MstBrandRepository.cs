using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MstBrandRepository : BaseRepository
    {
        public MstBrandRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<(List<MstBrandRead> Data, int Total, int Filtered)> FilterAsync(MstBrandFilter filter)
        {
            var query = GetAllQueryable();

            var total = await query.CountAsync();

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                string searchLower = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(searchLower) || x.Tag.ToLower().Contains(searchLower));
            }

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<MstBrand?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(b => b.Id == id && b.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstBrandRead>> GetAllAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
        }

        public async Task<MstBrand> AddAsync(MstBrand brand)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                brand.ApplicationId = applicationId.Value;
            }
            else if (brand.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(brand.ApplicationId);
            ValidateApplicationIdForEntity(brand, applicationId, isSystemAdmin);

            _context.MstBrands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<MstBrand> RawAddAsync(MstBrand brand)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            // Just basic validation or none? 'RawAddAsync' seemed to bypass in original, but we should be careful.
            // Original implementation:
            // _context.MstBrands.Add(brand);
            // await _context.SaveChangesAsync();
            // return brand;

            // Should we apply app context? Assuming Internal use might need specific handling or just bypass.
            // Keeping it simple as per original but safer?
            // The original RawAddAsync didn't check anything.
            _context.MstBrands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task UpdateAsync(MstBrand brand)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(brand.ApplicationId);
            ValidateApplicationIdForEntity(brand, applicationId, isSystemAdmin);

            // _context.MstBrands.Update(brand); // Optional
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var brand = await query.FirstOrDefaultAsync();

            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            brand.Status = 0;
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstBrandRead> ProjectToRead(IQueryable<MstBrand> query)
        {
            return query.AsNoTracking().Select(x => new MstBrandRead
            {
                Id = x.Id,
                Name = x.Name ?? "",
                Tag = x.Tag ?? "",
                Status = x.Status ?? 0,
                ApplicationId = x.ApplicationId
            });
        }

        public IQueryable<MstBrand> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(b => b.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstBrandRead>> GetAllExportAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
        }
    }
}

