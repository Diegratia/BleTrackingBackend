using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class EvacuationTransactionRepository : BaseRepository
    {
        public EvacuationTransactionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<EvacuationTransaction> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.EvacuationTransactions
                .Include(e => e.EvacuationAssemblyPoint)
                .Include(e => e.Member)
                .Include(e => e.Visitor)
                .Include(e => e.Security)
                .Include(e => e.Card)
                .Where(e => e.Status != 0)
                .AsSplitQuery();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<EvacuationTransactionRead> ProjectToRead(IQueryable<EvacuationTransaction> query)
        {
            return query
                .Select(e => new EvacuationTransactionRead
                {
                    Id = e.Id,
                    EvacuationAlertId = e.EvacuationAlertId,
                    EvacuationAssemblyPointId = e.EvacuationAssemblyPointId,
                    PersonCategory = e.PersonCategory,
                    MemberId = e.MemberId,
                    VisitorId = e.VisitorId,
                    SecurityId = e.SecurityId,
                    CardId = e.CardId,
                    PersonStatus = e.PersonStatus,
                    DetectedAt = e.DetectedAt,
                    LastDetectedAt = e.LastDetectedAt,
                    ConfirmedBy = e.ConfirmedBy,
                    ConfirmedAt = e.ConfirmedAt,
                    ConfirmationNotes = e.ConfirmationNotes,
                    ApplicationId = e.ApplicationId,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    UpdatedBy = e.UpdatedBy,
                    UpdatedAt = e.UpdatedAt,
                    Status = e.Status,
                    AssemblyPointName = e.EvacuationAssemblyPoint != null ? e.EvacuationAssemblyPoint.Name : null,
                    MemberName = e.Member != null ? e.Member.Name : null,
                    VisitorName = e.Visitor != null ? e.Visitor.Name : null,
                    SecurityName = e.Security != null ? e.Security.Name : null,
                    CardNumber = e.Card != null ? e.Card.CardNumber : null
                });
        }

        public async Task<EvacuationTransactionRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EvacuationTransaction?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EvacuationTransactionRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<List<EvacuationTransactionRead>> GetByAlertIdAsync(Guid alertId)
        {
            return await ProjectToRead(BaseEntityQuery())
                .Where(e => e.EvacuationAlertId == alertId)
                .ToListAsync();
        }

        public async Task<List<EvacuationTransaction>> GetByAlertIdEntitiesAsync(Guid alertId)
        {
            return await BaseEntityQuery()
                .Where(e => e.EvacuationAlertId == alertId)
                .ToListAsync();
        }

        public async Task<EvacuationTransaction?> GetByPersonAndAlertAsync(
            Guid alertId, Guid? memberId, Guid? visitorId, Guid? securityId)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(e => e.EvacuationAlertId == alertId
                    && e.MemberId == memberId
                    && e.VisitorId == visitorId
                    && e.SecurityId == securityId);
        }

        public async Task<(List<EvacuationTransactionRead> Data, int Total, int Filtered)> FilterAsync(EvacuationTransactionFilter filter)
        {
            var query = BaseEntityQuery();

            // Apply filters
            var alertIds = ExtractIds(filter.EvacuationAlertId);
            if (alertIds.Count > 0)
            {
                query = query.Where(x => alertIds.Contains(x.EvacuationAlertId));
            }

            var assemblyPointIds = ExtractIds(filter.EvacuationAssemblyPointId);
            if (assemblyPointIds.Count > 0)
            {
                query = query.Where(x => assemblyPointIds.Contains(x.EvacuationAssemblyPointId));
            }

            if (filter.PersonCategory.ValueKind == JsonValueKind.String)
            {
                var categoryStr = filter.PersonCategory.GetString();
                if (Enum.TryParse<EvacuationPersonCategory>(categoryStr, true, out var category))
                {
                    query = query.Where(x => x.PersonCategory == category);
                }
            }

            if (filter.PersonStatus.ValueKind == JsonValueKind.String)
            {
                var statusStr = filter.PersonStatus.GetString();
                if (Enum.TryParse<EvacuationPersonStatus>(statusStr, true, out var status))
                {
                    query = query.Where(x => x.PersonStatus == status);
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => (x.Member != null && x.Member.Name != null && x.Member.Name.ToLower().Contains(filter.Search.ToLower()))
                    || (x.Visitor != null && x.Visitor.Name != null && x.Visitor.Name.ToLower().Contains(filter.Search.ToLower()))
                    || (x.Security != null && x.Security.Name != null && x.Security.Name.ToLower().Contains(filter.Search.ToLower())));
            }

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task AddAsync(EvacuationTransaction entity)
        {
            _context.EvacuationTransactions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EvacuationTransaction entity)
        {
            _context.EvacuationTransactions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.EvacuationTransactions.FindAsync(id);
            if (entity != null)
            {
                entity.Status = 0;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCountByAlertAndStatusAsync(Guid alertId, EvacuationPersonStatus status)
        {
            return await BaseEntityQuery()
                .CountAsync(e => e.EvacuationAlertId == alertId && e.PersonStatus == status);
        }

        public async Task<List<EvacuationTransaction>> GetTransactionsByAssemblyPointAsync(Guid alertId, Guid assemblyPointId)
        {
            return await BaseEntityQuery()
                .Where(e => e.EvacuationAlertId == alertId && e.EvacuationAssemblyPointId == assemblyPointId)
                .ToListAsync();
        }

        // Ownership validation methods
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidAssemblyPointOwnershipAsync(
            Guid assemblyPointId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<EvacuationAssemblyPoint>(
                new[] { assemblyPointId }, applicationId);
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidAlertOwnershipAsync(
            Guid alertId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<EvacuationAlert>(
                new[] { alertId }, applicationId);
        }
    }
}
