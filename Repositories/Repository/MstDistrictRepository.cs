using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstDistrictRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstDistrictRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstDistrict> GetByIdAsync(Guid id)
        {
            return await _context.MstDistricts
                .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
        }

        public async Task<IEnumerable<MstDistrict>> GetAllAsync()
        {
            return await _context.MstDistricts
                .Where(d => d.Status != 0)
                .ToListAsync();
        }

        public async Task<MstDistrict> AddAsync(MstDistrict district)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == district.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {district.ApplicationId} not found.");

            _context.MstDistricts.Add(district);
            await _context.SaveChangesAsync();
            return district;
        }

        public async Task UpdateAsync(MstDistrict district)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == district.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {district.ApplicationId} not found.");

            // _context.MstDistricts.Update(district);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var district = await _context.MstDistricts
                .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
            if (district == null)
                throw new KeyNotFoundException("District not found");

            district.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}