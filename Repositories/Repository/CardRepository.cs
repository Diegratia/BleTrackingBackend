using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class CardRepository
    {
        private readonly BleTrackingDbContext _context;

        public CardRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<Card> GetByIdAsync(Guid id)
        {
            return await _context.Cards
                .Include(b => b.RegisteredMaskedArea)
                .FirstOrDefaultAsync(b => b.Id == id && b.StatusCard != false);
        }

        public async Task<IEnumerable<Card>> GetAllAsync()
        {
            return await _context.Cards
                .Include(b => b.RegisteredMaskedArea)
                .Include(b => b.Member)
                .Include(b => b.Visitor)
                .Where(b => b.StatusCard != false)
                .ToListAsync();
        }

        public async Task<Card> AddAsync(Card card)
        {
            _context.Cards.Add(card);
            card.StatusCard = true;
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task UpdateAsync(Card card)
        {
            // _context.MstBleReaders.Update(bleReader);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var card = await _context.Cards
                .FirstOrDefaultAsync(b => b.Id == id && b.StatusCard != false);
            if (card == null)
                throw new KeyNotFoundException("Visitor Card not found");

            card.StatusCard = false;
            await _context.SaveChangesAsync();
        }

        public IQueryable<Card> GetAllQueryable()
        {
            return _context.Cards
                .Include(b => b.Member)
                .Include(b => b.Visitor)
                .Include(b => b.RegisteredMaskedAreas)
                .Where(b => b.StatusCard != false)
                .AsQueryable();
        }
        
          public async Task<FloorplanMaskedArea> GetMaskedAreaByIdAsync(Guid id)
        {
            return await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

    }
}