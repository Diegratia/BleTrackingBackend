using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstAccessControlRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstAccessControlRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstAccessControl> GetByIdAsync(Guid id)
        {
            return await _context.MstAccessControls
                .Include(a => a.Brand)
                .Include(a => a.Integration)
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<IEnumerable<MstAccessControl>> GetAllAsync()
        {
            return await _context.MstAccessControls
                .Include(a => a.Brand)
                .Include(a => a.Integration)
                .Where(a => a.Status != 0)
                .ToListAsync();
        }

        public async Task<MstAccessControl> AddAsync(MstAccessControl accessControl)
        {
            _context.MstAccessControls.Add(accessControl);
            await _context.SaveChangesAsync();
            return accessControl;
        }

        public async Task UpdateAsync(MstAccessControl accessControl)
        {
            // _context.MstAccessControls.Update(accessControl);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var accessControl = await GetByIdAsync(id);
            if (accessControl == null)
                throw new KeyNotFoundException("Access Control not found");

            accessControl.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}