using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class CardAccessMaskedAreaRepository : BaseRepository
    {
        public CardAccessMaskedAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<CardAccessMaskedArea>> GetByCardAccessIdAsync(Guid cardAccessId)
        {
            return await _context.CardAccessMaskedAreas
                .Include(ca => ca.MaskedArea)
                .Where(ca => ca.CardAccessId == cardAccessId)
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<CardAccessMaskedArea> entities)
        {
            _context.CardAccessMaskedAreas.AddRange(entities);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByCardAccessIdAsync(Guid cardAccessId)
        {
            var list = await _context.CardAccessMaskedAreas
                .Where(ca => ca.CardAccessId == cardAccessId)
                .ToListAsync();

            _context.CardAccessMaskedAreas.RemoveRange(list);
            await _context.SaveChangesAsync();
        }
    }
}
