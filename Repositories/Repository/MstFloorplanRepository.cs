using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstFloorplanRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstFloorplanRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstFloorplan> GetByIdAsync(Guid id)
        {
            return await _context.MstFloorplans
                .FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);
        }

        public async Task<IEnumerable<MstFloorplan>> GetAllAsync()
        {
            return await _context.MstFloorplans
                .Where(f => f.Status != 0)
                .ToListAsync();
        }

        public async Task<MstFloorplan> AddAsync(MstFloorplan floorplan)
        {
            // Validasi FloorId
            var floor = await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == floorplan.FloorId && f.Status != 0);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {floorplan.FloorId} not found.");

            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == floorplan.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {floorplan.ApplicationId} not found.");

            _context.MstFloorplans.Add(floorplan);
            await _context.SaveChangesAsync();
            return floorplan;
        }

        public async Task UpdateAsync(MstFloorplan floorplan)
        {
            // Validasi FloorId
            var floor = await _context.MstFloors
                .FirstOrDefaultAsync(f => f.Id == floorplan.FloorId && f.Status != 0);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {floorplan.FloorId} not found.");

            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == floorplan.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {floorplan.ApplicationId} not found.");

            _context.MstFloorplans.Update(floorplan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var floorplan = await _context.MstFloorplans
                .FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);
            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found");

            floorplan.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}