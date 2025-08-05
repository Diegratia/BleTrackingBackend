using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstBleReaderRepository : BaseRepository
    {
        public MstBleReaderRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstBleReader?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(r => r.Status != 0 && r.Id == id)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("BLE Reader not found");
        }

        public async Task<IEnumerable<MstBleReader>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstBleReader> AddAsync(MstBleReader reader)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required");

                reader.ApplicationId = applicationId.Value;
            }
            else if (reader.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must set ApplicationId");
            }

            await ValidateApplicationIdAsync(reader.ApplicationId);
            ValidateApplicationIdForEntity(reader, applicationId, isSystemAdmin);

            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == reader.BrandId && b.Status != 0);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {reader.BrandId} not found.");

            _context.MstBleReaders.Add(reader);
            await _context.SaveChangesAsync();

            return reader;
        }

        public async Task UpdateAsync(MstBleReader reader)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(reader.ApplicationId);
            ValidateApplicationIdForEntity(reader, applicationId, isSystemAdmin);

            var brand = await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == reader.BrandId && b.Status != 0);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {reader.BrandId} not found.");

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBleReaders
                .Where(r => r.Id == id && r.Status != 0);

            var reader = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            if (reader == null)
                throw new KeyNotFoundException("BLE Reader not found or unauthorized");

            await _context.SaveChangesAsync();
        }

        public async Task<MstBrand?> GetBrandByIdAsync(Guid id)
        {
            return await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        public IQueryable<MstBleReader> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstBleReaders
                .Include(r => r.Brand)
                .Where(r => r.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstBleReader>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstBleReaders
                .Where(d => d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }
    }
}
