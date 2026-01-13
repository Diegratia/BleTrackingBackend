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
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatrolRouteService(
            PatrolRouteRepository repo,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PatrolRouteDto> CreateAsync(PatrolRouteCreateDto dto)
        {
            var entity = _mapper.Map<PatrolRoute>(dto);

            var areaIds = dto.PatrolAreaIds
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

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

            SetCreateAudit(entity);
            SetStartEndArea(entity);
            await _repo.AddAsync(entity);
            return _mapper.Map<PatrolRouteDto>(entity);
        }

        public async Task<PatrolRouteDto> UpdateAsync(Guid id, PatrolRouteUpdateDto dto)
        {
            var route = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException($"PatrolRoute with id {id} not found");

            var newAreaIds = dto.PatrolAreaIds
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            // hapus yang tidak ada
            route.PatrolRouteAreas.RemoveWhere(x =>
                !newAreaIds.Contains(x.PatrolAreaId));

            // existing
            var existingMap = route.PatrolRouteAreas
                .ToDictionary(x => x.PatrolAreaId);

            // tambah yang baru
            var existingIds = route.PatrolRouteAreas
                .Select(x => x.PatrolAreaId)
                .ToHashSet();

            // tambah & update order
            for (int i = 0; i < newAreaIds.Count; i++)
            {
                var areaId = newAreaIds[i];

                if (existingMap.TryGetValue(areaId, out var existing))
                {
                    existing.OrderIndex = i + 1; 
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = UsernameFormToken;
                }
                else
                {
                    route.PatrolRouteAreas.Add(new PatrolRouteAreas
                    {
                        PatrolAreaId = areaId,
                        PatrolRouteId = route.Id,
                        OrderIndex = i + 1,
                        EstimatedDistance = 0,
                        EstimatedTime = 0,
                        status = 1,
                        ApplicationId = route.ApplicationId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken,
                    });
                }
            }
            SetStartEndArea(route);
            SetUpdateAudit(route);
            await _repo.UpdateAsync();
            _mapper.Map(dto, route);
            return _mapper.Map<PatrolRouteDto>(route);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolRoute with id {id} not found");
            SetDeleteAudit(entity);
            await _repo.DeleteByRouteIdAsync(id);
            await _repo.DeleteAsync(id);
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
