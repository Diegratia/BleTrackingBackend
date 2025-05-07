using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class VisitorBlacklistAreaRepository
    {
        private readonly BleTrackingDbContext _context;

        public VisitorBlacklistAreaRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<List<VisitorBlacklistArea>> GetAllAsync()
        {
            return await _context.VisitorBlacklistAreas
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .ToListAsync();
        }

        public async Task<VisitorBlacklistArea> GetByIdAsync(Guid id)
        {
            return await _context.VisitorBlacklistAreas
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(VisitorBlacklistArea entity)
        {
            await _context.VisitorBlacklistAreas.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(VisitorBlacklistArea entity)
        {
            _context.VisitorBlacklistAreas.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> FloorplanMaskedAreaExists(Guid id)
        {
            return await _context.FloorplanMaskedAreas.AnyAsync(f => f.Id == id);
        }

        public async Task<bool> VisitorExists(Guid id)
        {
            return await _context.Visitors.AnyAsync(v => v.Id == id);
        }
    }
}
