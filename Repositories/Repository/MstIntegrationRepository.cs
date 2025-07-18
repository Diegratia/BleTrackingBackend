using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstIntegrationRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstIntegrationRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstBrand> GetBrandByIdAsync(Guid brandId)
        {
            return await _context.MstBrands
                .FirstOrDefaultAsync(b => b.Id == brandId && b.Status != 0);
        }

        public async Task<MstApplication> GetApplicationByIdAsync(Guid applicationId)
        {
            return await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicationStatus != 0);
        }

        public async Task<MstIntegration> GetByIdAsync(Guid id)
        {
            return await _context.MstIntegrations
                .Include(i => i.Brand)
                .FirstOrDefaultAsync(i => i.Id == id && i.Status != 0);
        }

        public async Task<IEnumerable<MstIntegration>> GetAllAsync()
        {
            return await _context.MstIntegrations
                .Include(i => i.Brand)
                .Where(i => i.Status != 0)
                .ToListAsync();
        }

        public async Task<MstIntegration> AddAsync(MstIntegration integration)
        {
            _context.MstIntegrations.Add(integration);
            await _context.SaveChangesAsync();
            return integration;
        }

        public async Task UpdateAsync(MstIntegration integration)
        {
            // _context.MstIntegrations.Update(integration);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var integration = await GetByIdAsync(id);
            if (integration == null)
                throw new KeyNotFoundException("Integration not found");

            integration.Status = 0;
            await _context.SaveChangesAsync();
        }
        public IQueryable<MstIntegration> GetAllQueryable()
        {
            return _context.MstIntegrations
                .Include(f => f.Brand)
                .Where(f => f.Status != 0)
                .AsQueryable();
        }
        
          public async Task<IEnumerable<MstIntegration>> GetAllExportAsync()
        {
            return await _context.MstIntegrations.Include(f => f.Brand).
            Where(f => f.Status != 0).ToListAsync();
        }  
        
    }
}