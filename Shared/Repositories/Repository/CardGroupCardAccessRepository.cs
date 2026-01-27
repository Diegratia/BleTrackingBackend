using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class CardGroupCardAccessRepository : BaseRepository
    {
        public CardGroupCardAccessRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        // public async Task<IEnumerable<CardGroupCardAccess>> GetByGroupIdAsync(Guid cardGroupId)
        // {
        //     return await _context.CardGroupCardAccesses
        //         .Include(cgca => cgca.CardAccess)
        //         .Where(cgca => cgca.CardGroupId == cardGroupId)
        //         .ToListAsync();
        // }

        // public async Task AddRangeAsync(IEnumerable<CardGroupCardAccess> entities)
        // {
        //     _context.CardGroupCardAccesses.AddRange(entities);
        //     await _context.SaveChangesAsync();
        // }

        // public async Task DeleteByGroupIdAsync(Guid cardGroupId)
        // {
        //     var list = await _context.CardGroupCardAccesses
        //         .Where(cgca => cgca.CardGroupId == cardGroupId)
        //         .ToListAsync();

        //     _context.CardGroupCardAccesses.RemoveRange(list);
        //     await _context.SaveChangesAsync();
        // }
    }
}
