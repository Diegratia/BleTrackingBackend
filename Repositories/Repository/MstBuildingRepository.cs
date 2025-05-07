using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstBuildingRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstBuildingRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstBuilding> GetByIdAsync(Guid id)
        {
            return await _context.MstBuildings
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        public async Task<IEnumerable<MstBuilding>> GetAllAsync()
        {
            return await _context.MstBuildings
                .Where(b => b.Status != 0)
                .ToListAsync();
        }

        public async Task<MstBuilding> AddAsync(MstBuilding building)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == building.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {building.ApplicationId} not found.");

            _context.MstBuildings.Add(building);
            await _context.SaveChangesAsync();
            return building;
        }

        public async Task UpdateAsync(MstBuilding building)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == building.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {building.ApplicationId} not found.");

            _context.MstBuildings.Update(building);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var building = await _context.MstBuildings
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
            if (building == null)
                throw new KeyNotFoundException("Building not found");

            building.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}