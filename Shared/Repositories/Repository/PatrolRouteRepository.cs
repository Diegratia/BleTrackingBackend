using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Repositories.Repository.RepoModel;
using System.Threading.Tasks;
using Shared.Contracts.Read;
using Shared.Contracts;
using Repositories.Extensions;

namespace Repositories.Repository
{
    public class PatrolRouteRepository : BaseRepository
    {
        public PatrolRouteRepository(
            BleTrackingDbContext context,
            IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }
        
        private IQueryable<PatrolRoute> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.PatrolRoutes
                .Where(x => x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query;
        }

            public IQueryable<PatrolRouteRead> ProjectToRead(IQueryable<PatrolRoute> query)
        {
            return query.AsNoTracking().Select(r => new PatrolRouteRead
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                ApplicationId = r.ApplicationId,
                PatrolAreas = r.PatrolRouteAreas
                    .Where(pra => pra.status != 0)
                    .OrderBy(pra => pra.OrderIndex)
                    .Select(pra => new PatrolRouteAreaReadDto
                    {
                        PatrolAreaId = pra.PatrolAreaId,
                        OrderIndex = pra.OrderIndex,
                        EstimatedDistance = pra.EstimatedDistance,
                        EstimatedTime = pra.EstimatedTime,
                        StartAreaId = pra.StartAreaId,
                        EndAreaId = pra.EndAreaId
                    }).ToList(),
                PatrolAreaCount = r.PatrolRouteAreas.Count(pra => pra.status != 0),
                StartAreaName = r.PatrolRouteAreas
                    .Where(pra => pra.status != 0)
                    .OrderBy(pra => pra.OrderIndex)
                    .Select(pra => pra.PatrolArea.Name)
                    .FirstOrDefault(),
                EndAreaName = r.PatrolRouteAreas
                    .Where(pra => pra.status != 0)
                    .OrderByDescending(pra => pra.OrderIndex)
                    .Select(pra => pra.PatrolArea.Name)
                    .FirstOrDefault()
            });
        }

          public async Task<(List<PatrolRouteRead> Data, int Total, int Filtered)> FilterAsync(PatrolRouteFilter filter)
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Description.ToLower().Contains(search)
                );
            }

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<PatrolRouteRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }
        
        public async Task<PatrolRoute?> GetByIdWithTrackingAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Include(x => x.PatrolRouteAreas)
                    .ThenInclude(x => x.PatrolArea)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<PatrolRouteRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public IQueryable<PatrolRoute> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolRoutes
                .Include(x => x.PatrolRouteAreas)
                    .ThenInclude(x => x.PatrolArea)
                .AsNoTracking()
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<List<PatrolRouteLookUpRead>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = BaseEntityQuery().AsNoTracking();

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var projected = query.Select(ca => new PatrolRouteLookUpRead
            {
                Id = ca.Id,
                Name = ca.Name,
                Description = ca.Description,
            });
            return await projected.ToListAsync();
        }

        public async Task<PatrolRoute> AddAsync(PatrolRoute entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required.");
                entity.ApplicationId = applicationId.Value;
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            _context.PatrolRoutes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }


        public async Task UpdateAsync(PatrolRoute patrolRoute)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolRoute.ApplicationId);
            ValidateApplicationIdForEntity(patrolRoute, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var entity = await _context.PatrolRoutes
                .FirstOrDefaultAsync(x => x.Id == id && x.Status != 0);
            entity.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByRouteIdAsync(Guid routeId)
        {
            var items = _context.PatrolRouteAreas
                .Where(x => x.PatrolRouteId == routeId);

            _context.PatrolRouteAreas.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        
        public void RemovePatrolRouteArea(PatrolRouteAreas entity)
        {
            _context.PatrolRouteAreas.Remove(entity);
        }


        public async Task<IReadOnlyCollection<Guid>> GetMissingAreaIdsAsync(
    IEnumerable<Guid> ids
        )
        {
            var idList = ids.Distinct().ToList();
            if (!idList.Any())
                return Array.Empty<Guid>();

            var existingIds = await _context.PatrolAreas
                .Where(x => idList.Contains(x.Id) && x.Status != 0)
                .Select(x => x.Id)
                .ToListAsync();

            return idList.Except(existingIds).ToList();
        }
    }
}
