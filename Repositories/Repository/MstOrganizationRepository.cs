using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstOrganizationRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstOrganizationRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MstOrganization>> GetAllAsync()
        {
            return await _context.MstOrganizations.ToListAsync();
        }

        public async Task<MstOrganization> GetByIdAsync(Guid id)
        {
            return await _context.MstOrganizations.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(MstOrganization organization)
        {
            _context.MstOrganizations.Add(organization);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstOrganization organization)
        {
            _context.MstOrganizations.Update(organization);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MstOrganization organization)
        {
            _context.MstOrganizations.Update(organization); // Soft delete logic
            await _context.SaveChangesAsync();
        }
    }
}
