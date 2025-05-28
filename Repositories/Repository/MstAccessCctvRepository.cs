using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstAccessCctvRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstAccessCctvRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstAccessCctv> GetByIdAsync(Guid id)
        {
            return await _context.MstAccessCctvs
                .Include(a => a.Integration)
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<IEnumerable<MstAccessCctv>> GetAllAsync()
        {
            return await _context.MstAccessCctvs
                .Include(a => a.Integration)
                .Where(a => a.Status != 0)
                .ToListAsync();
        }

        public async Task<MstAccessCctv> AddAsync(MstAccessCctv accessCctv)
        {
            _context.MstAccessCctvs.Add(accessCctv);
            await _context.SaveChangesAsync();
            return accessCctv;
        }

        public async Task UpdateAsync(MstAccessCctv accessCctv)
        {
            // _context.MstAccessCctvs.Update(accessCctv);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var accessCctv = await GetByIdAsync(id);
            if (accessCctv == null)
                throw new KeyNotFoundException("Access CCTV not found");

            accessCctv.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}