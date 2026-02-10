using AutoMapper;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolRouteService : BaseService, IPatrolRouteService
    {
        private readonly PatrolRouteRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;


        public PatrolRouteService(
            PatrolRouteRepository repo,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<PatrolRouteRead> CreateAsync(PatrolRouteCreateDto dto)
        {
            var entity = _mapper.Map<PatrolRoute>(dto);

            var areaIds = dto.PatrolAreaIds
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

                var missingAreaIds = await _repo
                .GetMissingAreaIdsAsync(areaIds);

            if (missingAreaIds.Count > 0)
            {
                throw new NotFoundException(
                    $"PatrolArea not found: {string.Join(", ", missingAreaIds)}"
                );
            }

            // 1. Load area shapes for distance calculation
            var areasLookup = await _repo.GetAreasWithShapeAsync(areaIds);

            // 2. Create route areas
            foreach (var (areaId, index) in areaIds.Select((id, i) => (id, i)))
            {
                entity.PatrolRouteAreas.Add(new PatrolRouteAreas
                {
                    PatrolAreaId = areaId,
                    ApplicationId = AppId,
                    OrderIndex = index + 1,
                    EstimatedDistance = 0,  // Temporary, will be calculated
                    EstimatedTime = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = UsernameFormToken,
                    UpdatedBy = UsernameFormToken,
                });
            }

            // 3. Calculate and set distances
            CalculateAndSetDistances(entity.PatrolRouteAreas, areasLookup);

            PatrolRouteRead result = null;

                await _repo.ExecuteInTransactionAsync(async () =>
                {
                    SetCreateAudit(entity);
                    SetStartEndArea(entity);
                    await _repo.AddAsync(entity);
                    result = await _repo.GetByIdAsync(entity.Id)
                        ?? throw new Exception("Failed to reload PatrolRoute");
                });
                 _audit.Created(
                    "Patrol Route",
                    result.Id,
                    "Created Patrol Route",
                    new { result.Name }
                );
                return result;
        }

        public async Task<PatrolRouteRead> UpdateAsync(Guid id, PatrolRouteUpdateDto dto)
        {

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                var route = await _repo.GetByIdWithTrackingAsync(id)
                    ?? throw new NotFoundException($"PatrolRoute with id {id} not found");
                // =====================================================
                // 🔥 REPLACE ALL AREAS
                // =====================================================
                var newAreaIds = dto.PatrolAreaIds?
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct()
                    .ToList() ?? new List<Guid>();
                

                foreach (var old in route.PatrolRouteAreas.ToList())
                {
                    _repo.RemovePatrolRouteArea(old); // state = Deleted
                }
                route.PatrolRouteAreas.Clear();

                // 1. Load area shapes for distance calculation
                var areasLookup = await _repo.GetAreasWithShapeAsync(newAreaIds);

                // 2. Create new route areas
                for (int i = 0; i < newAreaIds.Count; i++)
                {
                    route.PatrolRouteAreas.Add(new PatrolRouteAreas
                    {
                        PatrolRouteId = route.Id,
                        PatrolAreaId = newAreaIds[i],
                        OrderIndex = i + 1,
                        EstimatedDistance = 0,  // Temporary, will be calculated
                        EstimatedTime = 0,
                        status = 1,
                        ApplicationId = route.ApplicationId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken
                    });
                }

                // 3. Calculate and set distances
                CalculateAndSetDistances(route.PatrolRouteAreas, areasLookup);

                // =====================================================
                // 🔹 UPDATE ROUTE (SCALAR SAJA)
                // =====================================================
                _mapper.Map(dto, route); // navigation di-ignore
                SetStartEndArea(route);
                SetUpdateAudit(route);

                // 🔥 PASTIKAN EF UPDATE PARENT
                await _repo.UpdateAsync(route);
            });
                // reload untuk response
            var result = await _repo.GetByIdAsync(id)
                    ?? throw new Exception("Failed to reload PatrolRoute after update");

             _audit.Updated(
                "Patrol Route",
                result.Id,
                "Updated Patrol Route",
                new { result.Name }
            );
            return result;
        }
        


        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repo.GetByIdWithTrackingAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolRoute with id {id} not found");

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                SetDeleteAudit(entity);
                await _repo.DeleteByRouteIdAsync(id);
                await _repo.DeleteAsync(id);
            });
             _audit.Deleted(
                    "Patrol Route",
                    entity.Id,
                    "Deleted Patrol Route",
                    new
                    {
                        entity.Name
                    }
                );
        }

        public async Task<PatrolRouteRead?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolRoute with id {id} not found");
            return entity;
        }
            public async Task<IEnumerable<PatrolRouteRead?>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return entities;
        }
            public async Task<IEnumerable<PatrolRouteLookUpRead>> GetAllLookUpAsync()
        {
            var entities = await _repo.GetAllLookUpAsync();
            return entities;
        }

        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            PatrolRouteFilter filter
        )
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn;
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            if (request.DateFilters != null && request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
            {
                filter.DateFrom = dateFilter.DateFrom;
                filter.DateTo = dateFilter.DateTo;
            }

            var (data, total, filtered) = await _repo.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }


        private void SetStartEndArea(PatrolRoute route)
        {
            if (!route.PatrolRouteAreas.Any())
                return;

            var ordered = route.PatrolRouteAreas
                .Where(x => x.status != 0)
                .OrderBy(x => x.OrderIndex)
                .ToList();

            var startAreaId = ordered.First().PatrolAreaId;
            var endAreaId = ordered.Last().PatrolAreaId;

            foreach (var item in ordered)
            {
                item.StartAreaId = startAreaId;
                item.EndAreaId = endAreaId;
            }
        }

        /// <summary>
        /// Calculate and set estimated distances for route areas
        /// </summary>
        private void CalculateAndSetDistances(
            ICollection<PatrolRouteAreas> routeAreas,
            Dictionary<Guid, PatrolArea> areasLookup)
        {
            // Prepare data for geometry helper
            var orderedAreas = routeAreas
                .OrderBy(x => x.OrderIndex)
                .Select(x => (
                    AreaId: x.PatrolAreaId,
                    OrderIndex: x.OrderIndex,
                    AreaShape: areasLookup.TryGetValue(x.PatrolAreaId, out var area)
                        ? area.AreaShape
                        : null
                ))
                .ToList();

            // Calculate distances using geometry helper
            var distanceMap = GeometryHelper.CalculateRouteDistances(orderedAreas);

            // Apply calculated distances to route areas
            foreach (var routeArea in routeAreas)
            {
                routeArea.EstimatedDistance = distanceMap.TryGetValue(
                    routeArea.OrderIndex, out var distance)
                    ? distance
                    : 0;
            }
        }

    }
}
