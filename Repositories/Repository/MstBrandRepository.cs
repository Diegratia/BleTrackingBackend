using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstBrandRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstBrandRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstBrand> GetByIdAsync(Guid id)
        {
            return await _context.MstBrands
                .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
        }

        public async Task<IEnumerable<MstBrand>> GetAllAsync()
        {
            return await _context.MstBrands
            .Where(b => b.Status != 0)
            .ToListAsync();
        }

        public async Task AddAsync(MstBrand brand)
        {
            _context.MstBrands.Add(brand);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstBrand brand)
        {
            // _context.MstBrands.Update(brand);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MstBrand brand)
        {
            // _context.MstBrands.Update(brand); 
            await _context.SaveChangesAsync();
        }

            public IQueryable<MstBrand> GetAllQueryable()
        {
            return _context.MstBrands
                .Where(f => f.Status != 0)
                .AsQueryable();
        }

          public async Task<IEnumerable<MstBrand>> GetAllExportAsync()
        {
            return await _context.MstBrands
                .Where(d => d.Status != 0)
                .ToListAsync();
        }
    }
}