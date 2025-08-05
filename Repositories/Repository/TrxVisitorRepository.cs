using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;

namespace Repositories.Repository
{
    public class TrxVisitorRepository : BaseRepository
    {
        public TrxVisitorRepository(BleTrackingDbDevContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<TrxVisitor?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
                .Where(v => v.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TrxVisitor>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<TrxVisitor> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrxVisitors
                .Where(v => v.TrxStatus != 0)
                .Include(v => v.Application)
                .Include(v => v.Visitor)
                .Include(v => v.MaskedArea);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        } 

        public async Task<TrxVisitor> AddAsync(TrxVisitor trxVisitor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                trxVisitor.ApplicationId = applicationId.Value;
            }
            else if (trxVisitor.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(trxVisitor.ApplicationId);
            ValidateApplicationIdForEntity(trxVisitor, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(trxVisitor, applicationId, isSystemAdmin);

            await _context.TrxVisitors.AddAsync(trxVisitor);
            await _context.SaveChangesAsync();
            return trxVisitor;
        }

        public async Task UpdateAsync(TrxVisitor trxVisitor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(trxVisitor.ApplicationId);
            ValidateApplicationIdForEntity(trxVisitor, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(trxVisitor, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var trxVisitor = await _context.TrxVisitors.FirstOrDefaultAsync(t => t.Id == id);
            if (trxVisitor == null)
                throw new KeyNotFoundException("TrxVisitor not found");

            ValidateApplicationIdForEntity(trxVisitor, applicationId, isSystemAdmin);

            _context.TrxVisitors.Remove(trxVisitor);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> GetVisitorsAsync(Guid id)
        {
            return await _context.Visitors.AnyAsync(f => f.Id == id);
        }

        private async Task ValidateRelatedEntitiesAsync(TrxVisitor trxVisitor, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            if (trxVisitor.VisitorId != Guid.Empty)
            {
                var visitor = await _context.Visitors
                    .FirstOrDefaultAsync(v => v.Id == trxVisitor.VisitorId && v.ApplicationId == applicationId);

                if (visitor == null)
                    throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");
            }
        }
        
        public async Task<TrxVisitor?> GetLatestUnfinishedByVisitorIdAsync(Guid visitorId)
        {
            return await _context.TrxVisitors
                .Where(t => t.VisitorId == visitorId && t.Status != null && t.CheckedOutAt == null)
                .OrderByDescending(t => t.CheckedInAt)
                .FirstOrDefaultAsync();
        }
    }
}
