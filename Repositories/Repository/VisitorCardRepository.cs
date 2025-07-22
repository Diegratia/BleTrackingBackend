using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class VisitorCardRepository
    {
        private readonly BleTrackingDbContext _context;

        public VisitorCardRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<VisitorCard> GetByIdAsync(Guid id)
        {
            return await _context.VisitorCards
                .Include(b => b.Member)
                .Include(b => b.Card)
                .Include(b => b.Visitor)
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        public async Task<IEnumerable<VisitorCard>> GetAllAsync()
        {
            return await _context.VisitorCards
                .Include(b => b.Member)
                .Include(b => b.Card)
                .Include(b => b.Visitor)
                .Where(b => b.Status != 0)
                .ToListAsync();
        }

        public async Task<VisitorCard> AddAsync(VisitorCard visitorCard)
        {
            _context.VisitorCards.Add(visitorCard);
            await _context.SaveChangesAsync();
            return visitorCard;
        }

        public async Task UpdateAsync(VisitorCard visitorCard)
        {
            // _context.MstBleReaders.Update(bleReader);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var visitorCard = await _context.VisitorCards
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
            if (visitorCard == null)
                throw new KeyNotFoundException("Visitor Card not found");

            visitorCard.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<MstApplication> GetApplicationByIdAsync(Guid id)
        {
            return await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == id && a.ApplicationStatus != 0);
        }

             public async Task<Card> GetCardByIdAsync(Guid id)
        {
            return await _context.Cards
                .FirstOrDefaultAsync(a => a.Id == id && a.StatusCard != false && a.IsUsed != true);
        }
        
         public IQueryable<VisitorCard> GetAllQueryable()
        {
            return _context.VisitorCards
                .Include(b => b.Member)
                .Include(b => b.Card)
                .Include(b => b.Visitor)
                .Where(b => b.Status == 0)
                .AsQueryable();
        }

    }
}