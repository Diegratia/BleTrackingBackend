using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class VisitorRepository
    {
        private readonly BleTrackingDbContext _context;

        public VisitorRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Visitor>> GetAllAsync()
        {
            return await _context.Visitors
                .Include(v => v.Application)
                .ToListAsync();
        }

        public async Task<Visitor> GetByIdAsync(Guid id)
        {
            return await _context.Visitors
                .Include(v => v.Application)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(Visitor visitor)
        {
            await _context.Visitors.AddAsync(visitor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Visitor visitor)
        {
            // _context.Visitors.Remove(visitor);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ApplicationExists(Guid id)
        {
            return await _context.MstApplications.AnyAsync(f => f.Id == id);
        }

           public IQueryable<Visitor> GetAllQueryable()
        {
            return _context.Visitors
                .AsQueryable();
        }
    }
}
