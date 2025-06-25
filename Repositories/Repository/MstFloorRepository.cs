using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstFloorRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstFloorRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstFloor> GetByIdAsync(Guid id)
        {
            return await _context.MstFloors
                .Include(f => f.Building)
                .FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);
        }

        public async Task<IEnumerable<MstFloor>> GetAllAsync()
        {
            return await _context.MstFloors
                .Include(f => f.Building)
                .Where(f => f.Status != 0)
                .ToListAsync();
        }

        public async Task<MstFloor> AddAsync(MstFloor floor)
        {
            _context.MstFloors.Add(floor);
            await _context.SaveChangesAsync();
            return floor;
        }

        public async Task UpdateAsync(MstFloor floor)
        {
            // _context.MstFloors.Update(floor);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var floor = await GetByIdAsync(id);
            if (floor == null)
                throw new KeyNotFoundException("Floor not found");

            floor.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<MstBuilding> GetBuildingByIdAsync(Guid id)
        {
            return await _context.MstBuildings
                .FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);
        }
        
         public async Task<bool> NameExistsAsync(Guid buildingId, string name)
    {
        return await _context.MstFloors
            .AnyAsync(f => f.BuildingId == buildingId && f.Name == name && f.Status != 0);
    }
    }
}