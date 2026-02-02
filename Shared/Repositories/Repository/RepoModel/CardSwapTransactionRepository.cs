using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Extensions;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository.RepoModel
{
    public class CardSwapTransactionRepository : BaseRepository
    {
        public CardSwapTransactionRepository(
            BleTrackingDbContext context, 
            IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        private IQueryable<CardSwapTransaction> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardSwapTransactions
                .AsQueryable()
                .Where(e => e.ApplicationId == applicationId);
            
            return query;
        }

        public async Task<CardSwapTransactionRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<CardSwapTransaction?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery()
                .Where(x => x.Id == id)
                .Include(x => x.FromCard)
                .Include(x => x.ToCard)
                .Include(x => x.Visitor)
                .Include(x => x.MaskedArea);
            
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CardSwapTransactionRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public async Task<CardSwapTransaction> AddAsync(CardSwapTransaction entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");
                entity.ApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);

            _context.CardSwapTransactions.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(CardSwapTransaction entity)
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<CardSwapTransactionRead> ProjectToRead(IQueryable<CardSwapTransaction> query)
        {
            return query
                .AsNoTracking()
                .Select(t => new CardSwapTransactionRead
                {
                    Id = t.Id,
                    FromCardId = t.FromCardId,
                    FromCardNumber = t.FromCard != null ? t.FromCard.CardNumber ?? "N/A" : "N/A",
                    ToCardId = t.ToCardId,
                    ToCardNumber = t.ToCard != null ? t.ToCard.CardNumber ?? "N/A" : "N/A",
                    VisitorId = t.VisitorId,
                    VisitorName = t.Visitor != null ? t.Visitor.Name ?? "N/A" : "N/A",
                    TrxVisitorId = t.TrxVisitorId,
                    SwapType = t.SwapType ?? SwapType.EnterArea,
                    CardSwapStatus = t.CardSwapStatus ?? CardSwapStatus.Pending,
                    MaskedAreaId = t.MaskedAreaId ?? Guid.Empty,
                    MaskedAreaName = t.MaskedArea != null ? t.MaskedArea.Name ?? "N/A" : "N/A",
                    SwapChainId = t.SwapChainId ?? Guid.Empty,
                    SwapSequence = t.SwapSequence,
                    SwapBy = t.SwapBy ?? "N/A",
                    ExecutedAt = t.ExecutedAt,
                    CompletedAt = t.CompletedAt,
                    IdentityType = t.IdentityType,
                    IdentityValue = t.IdentityValue,
                    ApplicationId = t.ApplicationId, 
                });
        }

        public async Task<(List<CardSwapTransactionRead> Data, int Total, int Filtered)> FilterAsync(
            CardSwapTransactionFilter filter)
        {
            var baseQuery = BaseEntityQuery();

            // Apply includes after filtering for better performance
            var query = baseQuery;
                // .Include(x => x.FromCard)
                // .Include(x => x.ToCard)
                // .Include(x => x.Visitor)
                // .Include(x => x.MaskedArea);

            var total = await baseQuery.CountAsync();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x => 
                    (x.SwapBy != null && x.SwapBy.ToLower().Contains(search)) ||
                    (x.IdentityValue != null && x.IdentityValue.ToLower().Contains(search)) ||
                    (x.FromCard != null && x.FromCard.CardNumber != null && x.FromCard.CardNumber.ToLower().Contains(search)) ||
                    (x.ToCard != null && x.ToCard.CardNumber != null && x.ToCard.CardNumber.ToLower().Contains(search)) ||
                    (x.Visitor != null && x.Visitor.Name != null && x.Visitor.Name.ToLower().Contains(search))
                );
            }

            if (filter.VisitorId.HasValue)
                query = query.Where(x => x.VisitorId == filter.VisitorId.Value);

            if (filter.SwapChainId.HasValue)
                query = query.Where(x => x.SwapChainId == filter.SwapChainId.Value);

            if (filter.SwapType.HasValue)
                query = query.Where(x => x.SwapType == filter.SwapType.Value);

            if (filter.Status.HasValue)
                query = query.Where(x => x.CardSwapStatus == filter.Status.Value);

            if (filter.MaskedAreaId.HasValue)
                query = query.Where(x => x.MaskedAreaId == filter.MaskedAreaId.Value);

            if (filter.FromCardId.HasValue)
                query = query.Where(x => x.FromCardId == filter.FromCardId.Value);

            if (filter.ToCardId.HasValue)
                query = query.Where(x => x.ToCardId == filter.ToCardId.Value);

            var filtered = await query.CountAsync();

            // Sorting & Paging
            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        // Business specific queries
        public async Task<CardSwapTransaction?> GetLastActiveSwapAsync(Guid visitorId, Guid swapChainId)
        {
            var query = BaseEntityQuery()
                .Where(x => x.VisitorId == visitorId 
                    && x.CardSwapStatus == CardSwapStatus.Active
                    && x.SwapType == SwapType.EnterArea);
            
            if (swapChainId != Guid.Empty)
            {
                query = query.Where(x => x.SwapChainId == swapChainId);
            }
            
            query = query
                .Include(x => x.FromCard)
                .Include(x => x.ToCard)
                .Include(x => x.Visitor)
                .Include(x => x.MaskedArea)
                .OrderByDescending(x => x.SwapSequence);
            
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CardSwapTransaction>> GetActiveSwapsByChainAsync(Guid swapChainId)
        {
            var query = BaseEntityQuery()
                .Where(x => x.SwapChainId == swapChainId && x.CardSwapStatus == CardSwapStatus.Active)
                .OrderBy(x => x.SwapSequence)
                .Include(x => x.FromCard)
                .Include(x => x.ToCard)
                .Include(x => x.Visitor)
                .Include(x => x.MaskedArea);
            
            return await query.ToListAsync();
        }

        public async Task<int> GetNextSequenceAsync(Guid swapChainId)
        {
            var lastSequence = await BaseEntityQuery()
                .Where(x => x.SwapChainId == swapChainId)
                .MaxAsync(x => (int?)x.SwapSequence) ?? 0;
            
            return lastSequence + 1;
        }

        public async Task<Visitor?> GetVisitorByIdAsync(Guid visitorId)
        {
            return await _context.Visitors
                .FirstOrDefaultAsync(v => v.Id == visitorId && v.Status != 0);
        }

        public async Task<FloorplanMaskedArea?> GetMaskedAreaByIdAsync(Guid areaId)
        {
            return await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(a => a.Id == areaId && a.Status != 0);
        }
        public async Task<Card?> GetCardbyCardNumber(string? cardNumber)
        {
            return await _context.Cards
                .FirstOrDefaultAsync(a => a.CardNumber == cardNumber && a.StatusCard != 0);
        }
        public async Task<TrxVisitor?> GetLatestTrxVisitor(Guid? visitorId)
        {
            return await _context.TrxVisitors
                .FirstOrDefaultAsync(a => a.VisitorId == visitorId && a.CheckedInAt != null && a.CheckedOutAt == null);
        }
    }
}
