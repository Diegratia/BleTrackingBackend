using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class CardRecordRepository : BaseRepository
    {
        public CardRecordRepository(BleTrackingDbDevContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<CardRecord?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardRecords
                .Include(a => a.Visitor)
                .Include(a => a.Card)
                .Include(a => a.Member)
                .Include(a => a.Application)
                .Where(b => b.Id == id);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CardRecord>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<CardRecord> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardRecords
                .Include(a => a.Visitor)
                .Include(a => a.Card)
                .Include(a => a.Member)
                .Include(a => a.Application);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<CardRecord>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<Visitor?> GetVisitorByIdAsync(Guid id)
        {
            return await _context.Visitors
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<MstMember?> GetMemberByIdAsync(Guid id)
        {
            return await _context.MstMembers
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<Card?> GetCardByIdAsync(Guid id)
        {
            return await _context.Cards
                .FirstOrDefaultAsync(a => a.Id == id && a.StatusCard != 0);
        }
    }
}
