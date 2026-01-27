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
    public class MstBrandRepository : BaseRepository
    {
        public MstBrandRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstBrand> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstBrand>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(b => b.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }

//         public async Task<IEnumerable<MstBrand>> GetAllAsync()
// {
//     using (var connection = _connectionFactory.CreateConnection())
//     {
//         var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

//         var query = "SELECT * FROM MstBrands WHERE Status != @Status";
//         var parameters = new { Status = 0 };

//         if (!isSystemAdmin)
//         {
//             query += " AND (ApplicationId = @ApplicationId OR ApplicationId IS NULL)";
//             parameters["ApplicationId"] = applicationId;
//         }

//         var brands = await connection.QueryAsync<MstBrand>(query, parameters);

//         return brands;
//     }
// }

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

        public IQueryable<MstBrand> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstBrand>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBrands
                .Where(d => d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }
    }
}

