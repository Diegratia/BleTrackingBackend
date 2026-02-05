using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Shared.Contracts;
using Shared.Contracts.Read;


namespace Repositories.Repository
{
    public class MstEngineRepository : BaseRepository
    {
        public MstEngineRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        /// <summary>
        /// Base query with multi-tenancy and status filtering
        /// </summary>
        private IQueryable<MstEngine> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstEngines
                .Where(e => e.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        /// <summary>
        /// Manual projection to Read DTO (NOT using AutoMapper)
        /// </summary>
        private IQueryable<MstEngineRead> ProjectToRead(IQueryable<MstEngine> query)
        {
            return query.AsNoTracking()
                .Select(e => new MstEngineRead
                {
                    Id = e.Id,
                    Name = e.Name,
                    EngineTrackingId = e.EngineTrackingId,
                    Port = e.Port,
                    Status = e.Status ?? 0,
                    IsLive = e.IsLive,
                    LastLive = e.LastLive,
                    ServiceStatus = e.ServiceStatus.ToString(),
                    ApplicationId = e.ApplicationId,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    CreatedBy = e.CreatedBy,
                    UpdatedBy = e.UpdatedBy
                });
        }

        /// <summary>
        /// Get entity by ID for update/delete operations
        /// </summary>
        public async Task<MstEngine?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Get by ID with projection to Read DTO
        /// </summary>
        public async Task<MstEngineRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery()
                .Where(e => e.Id == id);

            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get by EngineTrackingId (string)
        /// </summary>
        public async Task<MstEngine?> GetByEngineIdAsync(string engineId)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(e => e.EngineTrackingId == engineId);
        }

        /// <summary>
        /// Get all engines with projection
        /// </summary>
        public async Task<List<MstEngineRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        /// <summary>
        /// Get all online engines
        /// </summary>
        public async Task<List<MstEngineRead>> GetAllOnlineAsync()
        {
            return await ProjectToRead(
                BaseEntityQuery()
                    .Where(e => e.ServiceStatus == ServiceStatus.Online)
            ).ToListAsync();
        }

        /// <summary>
        /// Filter engines with pagination
        /// </summary>
        public async Task<(List<MstEngineRead> Data, int Total, int Filtered)> FilterAsync(MstEngineFilter filter)
        {
            var query = BaseEntityQuery();

            // Search
            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(e => EF.Functions.Like(e.Name!, $"%{filter.Search}%"));
            }

            // Filter by Status
            if (filter.Status.HasValue)
            {
                query = query.Where(e => e.Status == filter.Status.Value);
            }

            // Filter by IsLive
            if (filter.IsLive.HasValue)
            {
                query = query.Where(e => e.IsLive == filter.IsLive.Value);
            }

            // Date range filter
            if (filter.DateFrom.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(e => e.CreatedAt <= filter.DateTo.Value);
            }

            var filtered = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(filter.SortColumn))
            {
                var sortColumn = char.ToUpper(filter.SortColumn[0]) + filter.SortColumn.Substring(1);
                query = filter.SortDir?.ToLower() == "asc"
                    ? query.OrderBy(e => EF.Property<object>(e, sortColumn))
                    : query.OrderByDescending(e => EF.Property<object>(e, sortColumn));
            }
            else
            {
                query = query.OrderByDescending(e => e.CreatedAt);
            }

            // Pagination
            var data = await ProjectToRead(query)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var total = await BaseEntityQuery().CountAsync();

            return (data, total, filtered);
        }

        /// <summary>
        /// Get all for export
        /// </summary>
        public async Task<List<MstEngineRead>> GetAllExportAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        /// <summary>
        /// Add new engine
        /// </summary>
        public async Task AddAsync(MstEngine engine)
        {
            _context.MstEngines.Add(engine);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update engine
        /// </summary>
        public async Task UpdateAsync(MstEngine engine)
        {
            _context.MstEngines.Update(engine);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Update by engine tracking ID string
        /// </summary>
        public async Task UpdateByEngineStringAsync(MstEngine engine)
        {
            _context.MstEngines.Attach(engine);
            _context.Entry(engine).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get queryable for legacy DataTables support
        /// </summary>
        public IQueryable<MstEngine> GetAllQueryable()
        {
            return BaseEntityQuery();
        }
    }
}
