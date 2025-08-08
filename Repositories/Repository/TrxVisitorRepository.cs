using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Data.ViewModels;

namespace Repositories.Repository
{
    public class TrxVisitorRepository : BaseRepository
    {
        public TrxVisitorRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<TrxVisitor?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
                .Where(v => v.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<TrxVisitor?> GetByPublicIdAsync(Guid id)
        {
            return await _context.TrxVisitors
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
                .Include(v => v.Application)
                .Include(v => v.Visitor)
                .Include(v => v.MaskedArea)
                .Include(v => v.Member);


            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<TrxVisitorDtoz> GetAllQueryableMinimal()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrxVisitors
                .Include(v => v.MaskedArea)
                .Include(v => v.Member)
                .AsQueryable();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query.Select(v => new TrxVisitorDtoz
            {
                Id = v.Id,
                CheckedInAt = v.CheckedInAt,
                Member = v.Member == null ? null : new MstMemberDtoz
                {
                    Id = v.Member.Id,
                    Name = v.Member.Name
                },
                Maskedarea = v.MaskedArea == null ? null : new FloorplanMaskedAreaDtoz
                {
                    Id = v.MaskedArea.Id,
                    Name = v.MaskedArea.Name
                }
            });
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

        public async Task UpdateAsyncRaw(TrxVisitor trxVisitor)
        {

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

        public async Task<int> CountByVisitorIdAsync(Guid visitorId)
        {
            return await _context.TrxVisitors.CountAsync(x => x.VisitorId == visitorId);
        }


        public async Task<TrxVisitor?> GetByInvitationCodeAsync(string invitationCode)
        {
            var trx = await _context.TrxVisitors
                .Include(t => t.Visitor)
                .FirstOrDefaultAsync(t =>
                    t.InvitationCode == invitationCode &&
                    t.InvitationTokenExpiredAt > DateTime.UtcNow);

            return trx;
        }

        public async Task<bool> ExistsOverlappingTrxAsync(Guid visitorId, DateTime start, DateTime end)
        {
            return await _context.TrxVisitors
                .Where(t =>
                    t.VisitorId == visitorId &&
                    t.TrxStatus != 0 &&
                    t.CheckedOutAt == null &&
                    t.IsInvitationAccepted == true &&
                    (
                        t.Status != VisitorStatus.Checkout ||
                        t.Status != VisitorStatus.Denied
                    )
                )
                .AnyAsync(t =>
                    t.VisitorPeriodStart != null &&
                    t.VisitorPeriodEnd != null &&
                    start <= t.VisitorPeriodEnd &&
                    end >= t.VisitorPeriodStart
                //                     .AnyAsync(t =>
                //     start == t.VisitorPeriodStart && end == t.VisitorPeriodEnd
                // );

                );
        }

        public async Task<string?> GetFloorNameByTrxIdAsync(Guid trxId)
        {
            return await _context.TrxVisitors
                .Where(t => t.Id == trxId)
                .Select(t => t.MaskedArea.Floor.Name)
                .FirstOrDefaultAsync();
        }
            
        public async Task<string?> GetBuildingNameByTrxIdAsync(Guid trxId)
            {
                return await _context.TrxVisitors
                    .Where(t => t.Id == trxId )
                    .Select(t => t.MaskedArea.Floor.Building.Name)
                    .FirstOrDefaultAsync();
            }

    }
}
