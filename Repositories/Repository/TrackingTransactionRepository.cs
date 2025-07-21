using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class TrackingTransactionRepository
    {
        private readonly BleTrackingDbContext _context;

        public TrackingTransactionRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<TrackingTransaction> GetByIdAsync(Guid id)
        {
            return await _context.TrackingTransactions.FindAsync(id);
        }

        public async Task<TrackingTransaction> GetByIdWithIncludesAsync(Guid id)
        {
            return await _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllWithIncludesAsync()
        {
            return await _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .ToListAsync();
        }
        

        public async Task AddAsync(TrackingTransaction transaction)
        {
            _context.TrackingTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrackingTransaction transaction)
        {
            // _context.TrackingTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TrackingTransaction transaction)
        {
            _context.TrackingTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public IQueryable<TrackingTransaction> GetAllQueryable()
        {
            return _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .AsQueryable();
        }

            public async Task<IEnumerable<TrackingTransaction>> GetAllExportAsync()
        {
            return await _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .ToListAsync();
        }
    }
}
