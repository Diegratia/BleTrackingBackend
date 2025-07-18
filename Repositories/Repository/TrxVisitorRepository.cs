using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class TrxVisitorRepository
    {
        private readonly BleTrackingDbContext _context;

        public TrxVisitorRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrxVisitor>> GetAllAsync()
        {
            return await _context.TrxVisitors
                .Include(v => v.Visitor)
                .ToListAsync();
        }

        public async Task<TrxVisitor> GetByIdAsync(Guid id)
        {
            return await _context.TrxVisitors
                .Include(v => v.Visitor)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(TrxVisitor trxVisitor)
        {
            await _context.TrxVisitors.AddAsync(trxVisitor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrxVisitor trxVisitor)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TrxVisitor trxVisitor)
        {
            // _context.Visitors.Remove(visitor);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetVisitorsAsync(Guid id)
        {
            return await _context.Visitors.AnyAsync(f => f.Id == id);
        }

           public IQueryable<TrxVisitor> GetAllQueryable()
        {
            return _context.TrxVisitors
            .Include(v => v.Visitor)
            .AsQueryable();
        }
    }
}
