using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class FloorplanMaskedAreaRepository
    {
        private readonly BleTrackingDbContext _context;

        public FloorplanMaskedAreaRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstFloor> GetFloorByIdAsync(Guid floorId)
        {
            return await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == floorId && f.Status != 0);
        }

        public async Task<FloorplanMaskedArea> GetByIdAsync(Guid id)
        {
            return await _context.FloorplanMaskedAreas
                .Include(a => a.Floor)
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<IEnumerable<FloorplanMaskedArea>> GetAllAsync()
        {
            return await _context.FloorplanMaskedAreas
                .Include(a => a.Floor)
                .Where(a => a.Status != 0)
                .ToListAsync();
        }

        public async Task<FloorplanMaskedArea> AddAsync(FloorplanMaskedArea area)
        {
            _context.FloorplanMaskedAreas.Add(area);
            await _context.SaveChangesAsync();
            return area;
        }

        public async Task UpdateAsync(FloorplanMaskedArea area)
        {
            // _context.FloorplanMaskedAreas.Update(area);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var area = await GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            area.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}