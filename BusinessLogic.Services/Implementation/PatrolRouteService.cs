using AutoMapper;
using BusinessLogic.Services.Extension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolRouteService : BaseService, IPatrolRouteService
    {
        private readonly PatrolRouteRepository _repo;
        private TimeGroupRepository _timeGroupRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;


        public PatrolRouteService(
            PatrolRouteRepository repo,
            TimeGroupRepository timeGroupRepository,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
            _timeGroupRepository = timeGroupRepository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<PatrolRouteDto> CreateAsync(PatrolRouteCreateDto dto)
        {
            var entity = _mapper.Map<PatrolRoute>(dto);

            var areaIds = dto.PatrolAreaIds
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            var TimeGroupIds = dto.TimeGroupIds
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

                var missingAreaIds = await _repo
                .GetMissingAreaIdsAsync(areaIds);

                var missingTimeGroupIds = await _repo
                .GetMissingTimeGroupIdsAsync(TimeGroupIds);

            if (missingAreaIds.Count > 0)
            {
                throw new NotFoundException(
                    $"PatrolArea not found: {string.Join(", ", missingAreaIds)}"
                );
            }

            if (missingTimeGroupIds.Count > 0)
            {
                throw new NotFoundException(
                    $"TimeGroup not found: {string.Join(", ", missingTimeGroupIds)}"
                );
            }

            entity.PatrolRouteTimeGroups = TimeGroupIds
                .Select(x => new PatrolRouteTimeGroups
                {
                    TimeGroupId = x,
                    ApplicationId = AppId,
                    PatrolRouteId = entity.Id,
                }).ToList();

            foreach (var (areaId, index) in areaIds.Select((id, i) => (id, i)))
            {
                entity.PatrolRouteAreas.Add(new PatrolRouteAreas
                {
                    PatrolAreaId = areaId,
                    ApplicationId = AppId,
                    OrderIndex = index + 1,          
                    EstimatedDistance = 0,         
                    EstimatedTime = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = UsernameFormToken,
                    UpdatedBy = UsernameFormToken,
                });
            }
            PatrolRoute result = null;

                await _repo.ExecuteInTransactionAsync(async () =>
                {
                    SetCreateAudit(entity);
                    SetStartEndArea(entity);
                    await _repo.AddAsync(entity);
                    result = await _repo.GetByIdAsync(entity.Id)
                        ?? throw new Exception("Failed to reload PatrolRoute");
                });
                await _audit.Created(
                    "Patrol Route",
                    result.Id,
                    "Created Patrol Route",
                    new { result.Name }
                );
                return _mapper.Map<PatrolRouteDto>(result);
        }

    public async Task<PatrolRouteDto> UpdateAsync(Guid id, PatrolRouteUpdateDto dto)
{
    PatrolRoute result = null;

    await _repo.ExecuteInTransactionAsync(async () =>
    {
        var route = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"PatrolRoute with id {id} not found");

        // =====================================================
        // 🔹 VALIDASI TIME GROUP
        // =====================================================
        var newTimeGroupIds = dto.TimeGroupIds?
            .Where(x => x.HasValue)
            .Select(x => x.Value)
            .Distinct()
            .ToList() ?? new List<Guid>();

        var missingTimeGroupIds =
            await _repo.GetMissingTimeGroupIdsAsync(newTimeGroupIds);

        if (missingTimeGroupIds.Count > 0)
            throw new NotFoundException(
                $"TimeGroup not found: {string.Join(", ", missingTimeGroupIds)}");

        // =====================================================
        // 🔥 REPLACE ALL TIME GROUPS
        // =====================================================
        foreach (var old in route.PatrolRouteTimeGroups.ToList())
        {
            _repo.RemovePatrolRouteTimeGroup(old); // state = Deleted
        }
        route.PatrolRouteTimeGroups.Clear();

        foreach (var tgId in newTimeGroupIds)
        {
            route.PatrolRouteTimeGroups.Add(new PatrolRouteTimeGroups
            {
                PatrolRouteId = route.Id,
                TimeGroupId = tgId,
                ApplicationId = route.ApplicationId,
                Status = 1
            });
        }

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

        for (int i = 0; i < newAreaIds.Count; i++)
        {
            route.PatrolRouteAreas.Add(new PatrolRouteAreas
            {
                PatrolRouteId = route.Id,
                PatrolAreaId = newAreaIds[i],
                OrderIndex = i + 1,
                EstimatedDistance = 0,
                EstimatedTime = 0,
                status = 1,
                ApplicationId = route.ApplicationId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = UsernameFormToken,
                UpdatedBy = UsernameFormToken
            });
        }

        // =====================================================
        // 🔹 UPDATE ROUTE (SCALAR SAJA)
        // =====================================================
        _mapper.Map(dto, route); // navigation di-ignore
        SetStartEndArea(route);
        SetUpdateAudit(route);

        // 🔥 PASTIKAN EF UPDATE PARENT
        await _repo.UpdateAsync(route);

        // reload untuk response
        result = await _repo.GetByIdAsync(route.Id)
            ?? throw new Exception("Failed to reload PatrolRoute after update");
    });

    await _audit.Updated(
        "Patrol Route",
        result.Id,
        "Updated Patrol Route",
        new { result.Name }
    );

    return _mapper.Map<PatrolRouteDto>(result);
}




        // public async Task<PatrolRouteDto> UpdateAsync(Guid id, PatrolRouteUpdateDto dto)
        // {
        //     var route = await _repo.GetByIdAsync(id)
        //         ?? throw new NotFoundException($"PatrolRoute with id {id} not found");

        //     // =======================
        //     // 🔹 UPDATE TIME GROUPS
        //     // =======================
        //     var newTimeGroupIds = dto.TimeGroupIds
        //         .Where(x => x.HasValue)
        //         .Select(x => x.Value)
        //         .Distinct()
        //         .ToList();

        //     // existing
        //     var existingTimeGroups = route.PatrolRouteTimeGroups
        //         .Where(x => x.Status != 0)
        //         .ToList();

        //     var existingTimeGroupIds = existingTimeGroups
        //         .Select(x => x.TimeGroupId)
        //         .ToHashSet();

        //     // 🔸 remove yang tidak ada di request
        //     var toRemove = existingTimeGroups
        //         .Where(x => !newTimeGroupIds.Contains(x.TimeGroupId))
        //         .ToList();

        //     foreach (var item in toRemove)
        //     {
        //         item.Status = 0; // soft delete
        //     }

        //     // 🔸 add yang baru
        //     var toAdd = newTimeGroupIds
        //         .Where(id => !existingTimeGroupIds.Contains(id))
        //         .ToList();

        //     foreach (var timeGroupId in toAdd)
        //     {
        //         route.PatrolRouteTimeGroups.Add(new PatrolRouteTimeGroups
        //         {
        //             PatrolRouteId = route.Id,
        //             TimeGroupId = timeGroupId,
        //             ApplicationId = route.ApplicationId,
        //             Status = 1,
        //         });
        //     }

        //     var newAreaIds = dto.PatrolAreaIds
        //         .Where(x => x.HasValue)
        //         .Select(x => x.Value)
        //         .Distinct()
        //         .ToList();

        //     // hapus yang tidak ada
        //     route.PatrolRouteAreas.RemoveWhere(x =>
        //         !newAreaIds.Contains(x.PatrolAreaId));

        //     // existing
        //     var existingMap = route.PatrolRouteAreas
        //         .ToDictionary(x => x.PatrolAreaId);

        //     // tambah yang baru
        //     var existingIds = route.PatrolRouteAreas
        //         .Select(x => x.PatrolAreaId)
        //         .ToHashSet();

        //     // tambah & update order
        //     for (int i = 0; i < newAreaIds.Count; i++)
        //     {
        //         var areaId = newAreaIds[i];

        //         if (existingMap.TryGetValue(areaId, out var existing))
        //         {
        //             existing.OrderIndex = i + 1; 
        //             existing.UpdatedAt = DateTime.UtcNow;
        //             existing.UpdatedBy = UsernameFormToken;
        //         }
        //         else
        //         {
        //             route.PatrolRouteAreas.Add(new PatrolRouteAreas
        //             {
        //                 PatrolAreaId = areaId,
        //                 PatrolRouteId = route.Id,
        //                 OrderIndex = i + 1,
        //                 EstimatedDistance = 0,
        //                 EstimatedTime = 0,
        //                 status = 1,
        //                 ApplicationId = route.ApplicationId,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedAt = DateTime.UtcNow,
        //                 CreatedBy = UsernameFormToken,
        //                 UpdatedBy = UsernameFormToken,
        //             });
        //         }
        //     }
        //     _mapper.Map(dto, route);
        //     SetStartEndArea(route);
        //     SetUpdateAudit(route);
        //     await _repo.UpdateAsync();
        //     await _audit.Updated(
        //         "Patrol Route",
        //         route.Id,
        //         "Updated Patrol Route",
        //         new { route.Name }
        //     );
        //     return _mapper.Map<PatrolRouteDto>(route);
        // }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolRoute with id {id} not found");

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                SetDeleteAudit(entity);
                entity.Status = 0;
                await _repo.DeleteByRouteIdAsync(id);
                await _repo.DeleteTimeGroupByRouteIdAsync(id);
                await _repo.DeleteAsync(id);
            });
            await _audit.Deleted(
                    "Patrol Route",
                    entity.Id,
                    "Deleted Patrol Route",
                    new { entity.Name
                    }
                );
        }

        public async Task<PatrolRouteDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolRoute with id {id} not found");
            return entity == null
                ? null
                : _mapper.Map<PatrolRouteDto>(entity);
        }
            public async Task<IEnumerable<PatrolRouteDto>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PatrolRouteDto>>(entities);
        }
            public async Task<IEnumerable<PatrolRouteDto>> GetAllLookUpAsync()
        {
            var entities = await _repo.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<PatrolRouteDto>>(entities);
        }

            public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repo.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Status", "OrderIndex", "Name" };

            var filterService = new GenericDataTableService< PatrolRoute, PatrolRouteDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
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

    }
}
