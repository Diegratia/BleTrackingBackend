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
    public class EvacuationAlertRepository : BaseRepository
    {
        public EvacuationAlertRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<EvacuationAlert> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.EvacuationAlerts
                .Include(e => e.Transactions)
                    .ThenInclude(t => t.EvacuationAssemblyPoint)
                .Where(e => e.Status != 0)
                .AsSplitQuery();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<EvacuationAlertRead> ProjectToRead(IQueryable<EvacuationAlert> query)
        {
            return query
                .Select(e => new EvacuationAlertRead
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    AlertStatus = e.AlertStatus,
                    TriggerType = e.TriggerType,
                    TriggeredBy = e.TriggeredBy,
                    StartedAt = e.StartedAt,
                    CompletedAt = e.CompletedAt,
                    CompletionNotes = e.CompletionNotes,
                    CompletedBy = e.CompletedBy,
                    TotalRequired = e.TotalRequired,
                    TotalEvacuated = e.TotalEvacuated,
                    TotalConfirmed = e.TotalConfirmed,
                    TotalRemaining = e.TotalRemaining,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    UpdatedBy = e.UpdatedBy,
                    UpdatedAt = e.UpdatedAt,
                    Status = e.Status,
                    ApplicationId = e.ApplicationId
                });
        }

        public async Task<EvacuationAlertRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EvacuationAlert?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<EvacuationAlertRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<EvacuationAlert?> GetActiveAlertAsync(Guid applicationId)
        {
            return await _context.EvacuationAlerts
                .Where(e => e.ApplicationId == applicationId
                    && e.Status != 0
                    && (e.AlertStatus == EvacuationAlertStatus.Active))
                .OrderByDescending(e => e.StartedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<(List<EvacuationAlertRead> Data, int Total, int Filtered)> FilterAsync(EvacuationAlertFilter filter)
        {
            var query = BaseEntityQuery();

            // Apply search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(x => (x.Title != null && x.Title.ToLower().Contains(filter.Search.ToLower()))
                    || (x.Description != null && x.Description.ToLower().Contains(filter.Search.ToLower())));
            }

            // Apply filters
            if (filter.AlertStatus.ValueKind != JsonValueKind.Undefined && filter.AlertStatus.ValueKind != JsonValueKind.Null)
            {
                if (filter.AlertStatus.ValueKind == JsonValueKind.String)
                {
                    var statusStr = filter.AlertStatus.GetString();
                    if (Enum.TryParse<EvacuationAlertStatus>(statusStr, true, out var status))
                    {
                        query = query.Where(x => x.AlertStatus == status);
                    }
                }
            }

            if (filter.TriggerType.ValueKind != JsonValueKind.Undefined && filter.TriggerType.ValueKind != JsonValueKind.Null)
            {
                if (filter.TriggerType.ValueKind == JsonValueKind.String)
                {
                    var typeStr = filter.TriggerType.GetString();
                    if (Enum.TryParse<EvacuationTriggerType>(typeStr, true, out var type))
                    {
                        query = query.Where(x => x.TriggerType == type);
                    }
                }
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(x => x.StartedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                var toDate = filter.DateTo.Value.AddDays(1);
                query = query.Where(x => x.StartedAt < toDate);
            }

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task AddAsync(EvacuationAlert entity)
        {
            _context.EvacuationAlerts.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EvacuationAlert entity)
        {
            _context.EvacuationAlerts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.EvacuationAlerts.FindAsync(id);
            if (entity != null)
            {
                entity.Status = 0;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCountersAsync(Guid alertId,
            int totalRequired, int totalEvacuated, int totalConfirmed, int totalRemaining)
        {
            var alert = await _context.EvacuationAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.TotalRequired = totalRequired;
                alert.TotalEvacuated = totalEvacuated;
                alert.TotalConfirmed = totalConfirmed;
                alert.TotalRemaining = totalRemaining;
                await _context.SaveChangesAsync();
            }
        }
    }
}
