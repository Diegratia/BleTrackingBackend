using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Repositories.Repository.RepoModel;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class CardRepository : BaseRepository
    {
        public CardRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<Card> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Cards
                .Include(c => c.RegisteredMaskedArea)
                .Include(c => c.Member)
                .Include(c => c.Visitor)
                .Include(c => c.Security)
                .Include(c => c.CardGroup)
                .Include(c => c.CardCardAccesses)
                    .ThenInclude(cga => cga.CardAccess)
                .Where(c => c.StatusCard != 0)
                .AsSplitQuery();

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<CardRead> ProjectToRead(IQueryable<Card> query)
        {
            return query
                .Select(c => new CardRead
                {
                    Id = c.Id,
                    Name = c.Name,
                    Remarks = c.Remarks,
                    CardType = c.CardType,
                    CardStatus = c.CardStatus,
                    CardNumber = c.CardNumber,
                    QRCode = c.QRCode,
                    Dmac = c.Dmac,
                    IsMultiMaskedArea = c.IsMultiMaskedArea,
                    RegisteredMaskedAreaId = c.RegisteredMaskedAreaId,
                    IsUsed = c.IsUsed,
                    LastUsed = c.LastUsed,
                    VisitorId = c.VisitorId,
                    MemberId = c.MemberId,
                    SecurityId = c.SecurityId,
                    CheckinAt = c.CheckinAt,
                    CheckoutAt = c.CheckoutAt,
                    StatusCard = c.StatusCard,
                    ApplicationId = c.ApplicationId,
                    CardGroupId = c.CardGroupId,
                    MemberName = c.Member != null ? c.Member.Name : null,
                    VisitorName = c.Visitor != null ? c.Visitor.Name : null,
                    SecurityName = c.Security != null ? c.Security.Name : null,
                    CardGroupName = c.CardGroup != null ? c.CardGroup.Name : null,
                    RegisteredMaskedAreaName = c.RegisteredMaskedArea != null ? c.RegisteredMaskedArea.Name : null,
                    CardAccesses = c.CardCardAccesses
                        .Where(ca => ca.CardAccess.Status != 0)
                        .Select(ca => new CardAccessRead
                        {
                            Id = ca.CardAccess.Id,
                            Name = ca.CardAccess.Name,
                            AccessNumber = ca.CardAccess.AccessNumber,
                            Remarks = ca.CardAccess.Remarks,
                            AccessScope = ca.CardAccess.AccessScope,
                            ApplicationId = ca.CardAccess.ApplicationId,
                            Status = ca.CardAccess.Status,
                            CreatedBy = ca.CardAccess.CreatedBy,
                            CreatedAt = ca.CardAccess.CreatedAt,
                            UpdatedBy = ca.CardAccess.UpdatedBy,
                            UpdatedAt = ca.CardAccess.UpdatedAt
                        }).ToList(),
                    CreatedBy = c.CreatedBy,
                    CreatedAt = c.CreatedAt,
                    UpdatedBy = c.UpdatedBy,
                    UpdatedAt = c.UpdatedAt,
                    Status = c.StatusCard ?? 0
                });
        }

        public async Task<CardRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Card?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Card?> GetByCardNumberAsync(string CardNumber)
        {

            return await GetAllQueryable()
            .Where(b => b.CardNumber == CardNumber && b.StatusCard != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CardRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<List<CardDashboardRM>> GetTopUnUsedCardAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.Cards
                .AsNoTracking()
                .Include(c => c.RegisteredMaskedArea)
                    .ThenInclude(ma => ma.Floorplan)
                        .ThenInclude(fp => fp.Floor)
                .Where(c => c.StatusCard != 0 && (c.IsUsed == false || c.IsUsed == null));

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            // Apply building filter untuk PrimaryAdmin
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                q = q.Where(c => c.RegisteredMaskedArea != null
                    && c.RegisteredMaskedArea.Floorplan != null
                    && c.RegisteredMaskedArea.Floorplan.Floor != null
                    && accessibleBuildingIds.Contains(c.RegisteredMaskedArea.Floorplan.Floor.BuildingId));
            }

            return await q
                .OrderByDescending(x => x.UpdatedAt)
                .Take(topCount)
                .Select(x => new CardDashboardRM
                {
                    Id = x.Id,
                    Dmac = x.Dmac ?? "Unknown Card",
                    CardNumber = x.CardNumber ?? "Unknown Card",
                })
                .ToListAsync();
        }

        public async Task<List<CardDashboardRM>> GetTopUsedCardAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.Cards
                .AsNoTracking()
                .Include(c => c.RegisteredMaskedArea)
                    .ThenInclude(ma => ma.Floorplan)
                        .ThenInclude(fp => fp.Floor)
                .Where(c => c.StatusCard != 0 && c.IsUsed == true);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            // Apply building filter untuk PrimaryAdmin
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                q = q.Where(c => c.RegisteredMaskedArea != null
                    && c.RegisteredMaskedArea.Floorplan != null
                    && c.RegisteredMaskedArea.Floorplan.Floor != null
                    && accessibleBuildingIds.Contains(c.RegisteredMaskedArea.Floorplan.Floor.BuildingId));
            }

            return await q
                .OrderByDescending(x => x.UpdatedAt)
                .Take(topCount)
                .Select(x => new CardDashboardRM
                {
                    Id = x.Id,
                    Dmac = x.Dmac ?? "Unknown Card",
                    CardNumber = x.CardNumber ?? "Unknown Card",
                })
                .ToListAsync();
        }
        
       public async Task<int> GetCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.Cards
                .AsNoTracking()
                .Include(c => c.RegisteredMaskedArea)
                    .ThenInclude(ma => ma.Floorplan)
                        .ThenInclude(fp => fp.Floor)
                .Where(c => c.StatusCard != 0 && c.IsUsed == true);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);
            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                q = q.Where(c => c.RegisteredMaskedArea != null
                    && c.RegisteredMaskedArea.Floorplan != null
                    && c.RegisteredMaskedArea.Floorplan.Floor != null
                    && accessibleBuildingIds.Contains(c.RegisteredMaskedArea.Floorplan.Floor.BuildingId));
            }

            return await q.CountAsync();
        }
        public async Task<CardUsageCountRM> CardUsageCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var q = _context.Cards
                .AsNoTracking()
                .Where(c => c.StatusCard != 0);

            var visitorUse = await q.Where(c => c.VisitorId != null && c.IsUsed == true).CountAsync();
            var memberUse = await q.Where(c => c.MemberId != null && c.IsUsed == true).CountAsync();
            var totalCard = await q.CountAsync();
            var totalUse = visitorUse + memberUse;

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return new CardUsageCountRM
            {
                VisitorCardCount = visitorUse,
                MemberCardCount = memberUse,
                TotalCardCount = totalCard,
                TotalCardUse = totalUse
            };
        }

         public async Task<int> GetCountEachIdAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.Cards
                .AsNoTracking()
                .Where(c => c.StatusCard != 0 && c.IsUsed == true);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q.CountAsync();
        }

        public async Task<int> GetNonActiveCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.Cards
                .AsNoTracking()
                .Include(c => c.RegisteredMaskedArea)
                    .ThenInclude(ma => ma.Floorplan)
                        .ThenInclude(fp => fp.Floor)
                .Where(c => c.StatusCard != 0 && (c.IsUsed == false || c.IsUsed == null));

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            var accessibleBuildingIds = GetAccessibleBuildingsFromToken();
            if (accessibleBuildingIds.Any())
            {
                q = q.Where(c => c.RegisteredMaskedArea != null
                    && c.RegisteredMaskedArea.Floorplan != null
                    && c.RegisteredMaskedArea.Floorplan.Floor != null
                    && accessibleBuildingIds.Contains(c.RegisteredMaskedArea.Floorplan.Floor.BuildingId));
            }

            return await q.CountAsync();
        }


            public async Task<IEnumerable<CardRead>> GetUnUsedCardAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.Cards
                .AsNoTracking()
                .Where(c => c.StatusCard != 0 && (c.IsUsed == false || c.IsUsed == null));

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);
            q.WithActiveRelations();

            return await ProjectToRead(q).ToListAsync();
        }

        
       

        public IQueryable<Card> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Cards
                .Include(b => b.RegisteredMaskedArea)
                .Include(b => b.Member)
                .Include(b => b.Visitor)
                .Include(b => b.Security)
                .Include(b => b.CardGroup)
                // .Include(b => b.Application)
                .Include(b => b.CardCardAccesses)
                        .ThenInclude(cga => cga.CardAccess)
                .Where(b => b.StatusCard != 0)
                .AsSplitQuery();

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<Card> AddAsync(Card card)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                card.ApplicationId = applicationId.Value;
            }
            else if (card.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(card.ApplicationId);
            ValidateApplicationIdForEntity(card, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(card, applicationId, isSystemAdmin);

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task UpdateAsync(Card card)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(card.ApplicationId);
            ValidateApplicationIdForEntity(card, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(card, applicationId, isSystemAdmin);

            // _context.Cards.Update(card);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<CardRead> Data, int Total, int Filtered)> FilterAsync(CardFilter filter)
        {
            var query = BaseEntityQuery();
            var total = await query.CountAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(c =>
                    (c.Name != null && c.Name.Contains(filter.Search)) ||
                    (c.CardNumber != null && c.CardNumber.Contains(filter.Search)) ||
                    (c.Dmac != null && c.Dmac.Contains(filter.Search)));
            }

            if (!string.IsNullOrEmpty(filter.CardNumber))
                query = query.Where(c => c.CardNumber != null && c.CardNumber.Contains(filter.CardNumber));

            if (!string.IsNullOrEmpty(filter.Dmac))
                query = query.Where(c => c.Dmac != null && c.Dmac.Contains(filter.Dmac));

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(c => c.Name != null && c.Name.Contains(filter.Name));

            if (filter.CardType.HasValue)
                query = query.Where(c => c.CardType == filter.CardType.Value);

            if (filter.CardStatus.HasValue)
                query = query.Where(c => c.CardStatus == filter.CardStatus.Value);

            if (filter.IsUsed.HasValue)
                query = query.Where(c => c.IsUsed == filter.IsUsed.Value);

            if (filter.IsMultiMaskedArea.HasValue)
                query = query.Where(c => c.IsMultiMaskedArea == filter.IsMultiMaskedArea.Value);

            if (filter.RegisteredMaskedAreaId.HasValue)
                query = query.Where(c => c.RegisteredMaskedAreaId == filter.RegisteredMaskedAreaId.Value);

            if (filter.StatusCard.HasValue)
                query = query.Where(c => c.StatusCard == filter.StatusCard.Value);

            // Filter by MemberId (supports both single Guid and Guid array)
            if (filter.MemberId.ValueKind != JsonValueKind.Undefined && filter.MemberId.ValueKind != JsonValueKind.Null)
            {
                var memberIds = ExtractIds(filter.MemberId);
                if (memberIds.Any())
                    query = query.Where(c => c.MemberId.HasValue && memberIds.Contains(c.MemberId.Value));
            }

            if (filter.VisitorId.ValueKind != JsonValueKind.Undefined && filter.VisitorId.ValueKind != JsonValueKind.Null)
            {
                var visitorIds = ExtractIds(filter.VisitorId);
                if (visitorIds.Any())
                    query = query.Where(c => c.VisitorId.HasValue && visitorIds.Contains(c.VisitorId.Value));
            }

            if (filter.SecurityId.ValueKind != JsonValueKind.Undefined && filter.SecurityId.ValueKind != JsonValueKind.Null)
            {
                var securityIds = ExtractIds(filter.SecurityId);
                if (securityIds.Any())
                    query = query.Where(c => c.SecurityId.HasValue && securityIds.Contains(c.SecurityId.Value));
            }

            if (filter.CardGroupId.ValueKind != JsonValueKind.Undefined && filter.CardGroupId.ValueKind != JsonValueKind.Null)
            {
                var cardGroupIds = ExtractIds(filter.CardGroupId);
                if (cardGroupIds.Any())
                    query = query.Where(c => c.CardGroupId.HasValue && cardGroupIds.Contains(c.CardGroupId.Value));
            }

            if (filter.DateFrom.HasValue)
                query = query.Where(c => c.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(c => c.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);

            if (string.IsNullOrEmpty(filter.SortColumn))
            {
                query = query.OrderByDescending(c => c.UpdatedAt);
            }

            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var card = await _context.Cards.FirstOrDefaultAsync(b => b.Id == id && b.StatusCard != 0);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            if (!isSystemAdmin && card.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don't have permission to delete this entity.");

            await _context.SaveChangesAsync();
        }

        public async Task<FloorplanMaskedArea?> GetMaskedAreaByIdAsync(Guid id)
        {
            return await _context.FloorplanMaskedAreas
                // .WithActiveRelations()
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        public async Task<IEnumerable<CardRead>> GetAllExportAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
        }

            public async Task<Card?> GetBleCardNumberAsync(string cardNumber)
        {
            return await GetAllQueryable()
            .Where(x => x.StatusCard != 0 && x.CardNumber == cardNumber)
            .FirstOrDefaultAsync();
        }

        public async Task AssignCardAccessAsync(Guid cardId, IEnumerable<Guid> cardAccessIds, string username)
        {
            var card = await _context.Cards
                .Include(c => c.CardCardAccesses)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found");

            // Hapus relasi lama
            card.CardCardAccesses.Clear();

            // Tambahkan yang baru
            foreach (var accessId in cardAccessIds.Distinct())
            {
                card.CardCardAccesses.Add(new CardCardAccess
                {
                    CardId = card.Id,
                    CardAccessId = accessId,
                    ApplicationId = card.ApplicationId,
                    Status = 1
                });
            }

            await _context.SaveChangesAsync();
        }


        private async Task ValidateRelatedEntitiesAsync(Card card, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            if (card.MemberId.HasValue)
            {
                var member = await _context.MstMembers
                    .WithActiveRelations()
                    .FirstOrDefaultAsync(m => m.Id == card.MemberId && m.ApplicationId == applicationId);

                if (member == null)
                    throw new UnauthorizedAccessException("Member not found or not accessible in your application.");
            }

            if (card.VisitorId.HasValue)
            {
                var visitor = await _context.Visitors
                    .WithActiveRelations()
                    .FirstOrDefaultAsync(v => v.Id == card.VisitorId && v.ApplicationId == applicationId);

                if (visitor == null)
                    throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");
            }
        }

        // Ownership validation helpers for service layer
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidMemberOwnershipAsync(
            Guid memberId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstMember>(
                new[] { memberId },
                applicationId
            );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidVisitorOwnershipAsync(
            Guid visitorId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<Visitor>(
                new[] { visitorId },
                applicationId
            );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidCardGroupOwnershipAsync(
            Guid cardGroupId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<CardGroup>(
                new[] { cardGroupId },
                applicationId
            );
        }
    }
}
