using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class CardRecordRepository
    {
        private readonly BleTrackingDbContext _context;

        public CardRecordRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<CardRecord> GetByIdAsync(Guid id)
        {
            return await _context.CardRecords
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<CardRecord>> GetAllAsync()
        {
            return await _context.CardRecords
                .ToListAsync();
        }

        public async Task<CardRecord> AddAsync(CardRecord cardRecord)
        {
            _context.CardRecords.Add(cardRecord);
            await _context.SaveChangesAsync();
            return cardRecord;
        }

        public async Task UpdateAsync(CardRecord cardRecord)
        {
            // _context.MstBleReaders.Update(bleReader);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Visitor> GetVisitorByIdAsync(Guid id)
        {
            return await _context.Visitors
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }
        
           public async Task<MstMember> GetMemberByIdAsync(Guid id)
        {
            return await _context.MstMembers
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

           public async Task<VisitorCard> GetCardByIdAsync(Guid id)
        {
            return await _context.VisitorCards
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

    }
}