using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer.DtoHelpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class CardAccessService : BaseService, ICardAccessService
    {
        private readonly CardAccessRepository _repository;
        private readonly IMapper _mapper;
        private readonly FloorplanMaskedAreaRepository _areaRepository;
        private readonly TimeGroupRepository _timeGroupRepository;
        private readonly CardRepository _cardRepository;
        private readonly IAuditEmitter _audit;

        public CardAccessService(
            CardAccessRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            FloorplanMaskedAreaRepository areaRepository,
            TimeGroupRepository timeGroupRepository,
            CardRepository cardRepository,
            IAuditEmitter audit
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _areaRepository = areaRepository;
            _timeGroupRepository = timeGroupRepository;
            _cardRepository = cardRepository;
            _audit = audit;
        }

        public async Task<CardAccessDto> CreateAsync(CardAccessCreateDto dto)
        {
            var entity = _mapper.Map<CardAccess>(dto);
            entity.Id = Guid.NewGuid();
            SetCreateAudit(entity);

            int? currentMax = _repository.GetAllQueryable().Max(m => m.AccessNumber);
            entity.AccessNumber = currentMax.HasValue ? currentMax.Value + 1 : 1;

            // 🔹 Resolve MaskedAreas based on location
            var resolvedMaskedAreaIds = await _areaRepository.GetMaskedAreaIdsByLocationAsync(
                dto.BuildingId, dto.FloorId, dto.FloorplanId
            );

            // Combine with manual MaskedAreaIds from input
            if (dto.MaskedAreaIds?.Any() == true)
            {
                resolvedMaskedAreaIds.AddRange(dto.MaskedAreaIds.Where(x => x.HasValue).Select(x => x.Value));
            }

            resolvedMaskedAreaIds = resolvedMaskedAreaIds.Distinct().ToList();

            if (resolvedMaskedAreaIds.Any())
            {
                entity.CardAccessMaskedAreas = resolvedMaskedAreaIds
                    .Select(id => new CardAccessMaskedArea
                    {
                        CardAccessId = entity.Id,
                        MaskedAreaId = id,
                        ApplicationId = AppId
                    }).ToList();
            }

            // 🔹 TimeGroups with ownership validation
            if (dto.TimeGroupIds.Any())
            {
                var invalidIds = await _timeGroupRepository.CheckInvalidTimeGroupOwnershipAsync(
                    dto.TimeGroupIds, AppId);

                if (invalidIds.Any())
                    throw new UnauthorizedException(
                        $"TimeGroupIds do not belong to this Application: {string.Join(", ", invalidIds)}");

                entity.CardAccessTimeGroups = dto.TimeGroupIds
                    .Where(x => x.HasValue)
                    .Select(id => new CardAccessTimeGroups
                    {
                        CardAccessId = entity.Id,
                        TimeGroupId = id.Value,
                        ApplicationId = AppId
                    }).ToList();
            }

            await _repository.AddAsync(entity);
            _audit.Created("CardAccess", entity.Id, $"CardAccess {entity.Name} created");

            var dtoResult = _mapper.Map<CardAccessDto>(entity);
            dtoResult.MaskedAreaIds = entity.CardAccessMaskedAreas?
                .Select(x => (Guid?)x.MaskedAreaId)
                .ToList();
            dtoResult.TimeGroupIds = entity.CardAccessTimeGroups?
                .Select(x => (Guid?)x.TimeGroupId)
                .ToList();

            return dtoResult;
        }

        public async Task UpdateAsync(Guid id, CardAccessUpdateDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("CardAccess not found");

            _mapper.Map(dto, entity);
            SetUpdateAudit(entity);

            // Update masked areas
            entity.CardAccessMaskedAreas.Clear();

            var resolvedMaskedAreaIds = await _areaRepository.GetMaskedAreaIdsByLocationAsync(
                dto.BuildingId, dto.FloorId, dto.FloorplanId
            );

            if (dto.MaskedAreaIds?.Any() == true)
            {
                resolvedMaskedAreaIds.AddRange(dto.MaskedAreaIds.Where(x => x.HasValue).Select(x => x.Value));
            }

            resolvedMaskedAreaIds = resolvedMaskedAreaIds.Distinct().ToList();

            if (resolvedMaskedAreaIds.Any())
            {
                entity.CardAccessMaskedAreas = resolvedMaskedAreaIds
                    .Select(id => new CardAccessMaskedArea
                    {
                        CardAccessId = entity.Id,
                        MaskedAreaId = id,
                        ApplicationId = entity.ApplicationId
                    }).ToList();
            }

            // 🔹 TimeGroups with ownership validation (if provided)
            if (dto.TimeGroupIds != null)
            {
                var invalidIds = await _timeGroupRepository.CheckInvalidTimeGroupOwnershipAsync(
                    dto.TimeGroupIds, AppId);

                if (invalidIds.Any())
                    throw new UnauthorizedException(
                        $"TimeGroupIds do not belong to this Application: {string.Join(", ", invalidIds)}");

                entity.CardAccessTimeGroups.Clear();
                foreach (var timeGroupId in dto.TimeGroupIds.Where(x => x.HasValue))
                {
                    entity.CardAccessTimeGroups.Add(new CardAccessTimeGroups
                    {
                        CardAccessId = entity.Id,
                        TimeGroupId = timeGroupId.Value,
                        ApplicationId = AppId
                    });
                }
            }

            await _repository.UpdateAsync(entity);
            _audit.Updated("CardAccess", entity.Id, $"CardAccess {entity.Name} updated");
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("CardAccess not found");

            SetDeleteAudit(entity);

            await _repository.DeleteAsync(id);
            _audit.Deleted("CardAccess", id, $"CardAccess {entity.Name} deleted");
        }

        public async Task<IEnumerable<CardAccessDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<CardAccessDto>>(entities);

            foreach (var dto in dtos)
            {
                var entity = entities.First(e => e.Id == dto.Id);
                dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                    .Select(x => (Guid?)x.MaskedAreaId)
                    .ToList();
                dto.TimeGroupIds = entity.CardAccessTimeGroups
                    .Select(x => (Guid?)x.TimeGroupId)
                    .ToList();
            }

            return dtos;
        }

        public async Task<IEnumerable<CardAccessOpenDto>> OpenGetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<CardAccessOpenDto>>(entities);

            foreach (var dto in dtos)
            {
                var entity = entities.First(e => e.Id == dto.Id);
                dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                    .Select(x => (Guid?)x.MaskedAreaId)
                    .ToList();
                dto.TimeGroupIds = entity.CardAccessTimeGroups
                    .Select(x => (Guid?)x.TimeGroupId)
                    .ToList();
            }

            return dtos;
        }

        public async Task<CardAccessDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            var dto = _mapper.Map<CardAccessDto>(entity);
            dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                .Select(x => (Guid?)x.MaskedAreaId)
                .ToList();
            dto.TimeGroupIds = entity.CardAccessTimeGroups
                    .Select(x => (Guid?)x.TimeGroupId)
                    .ToList();

            return dto;
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.MinimalGetAllQueryableDto();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Status", "AccessNumber", "AccessScope" };

            var filterService = new MinimalGenericDataTableService<CardAccessMinimalDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task AssignCardAccessToCardAsync(CardAssignAccessDto dto)
        {
            var allAccessIds = new HashSet<Guid>();

            // Manual CardAccessIds from input
            if (dto.CardAccessIds?.Any() == true)
            {
                foreach (var id in dto.CardAccessIds)
                    allAccessIds.Add(id);
            }

            // From location (building/floor/floorplan)
            var locationAccessIds = await _repository.GetCardAccessIdsByLocationAsync(
                dto.BuildingId, dto.FloorId, dto.FloorplanId);

            foreach (var id in locationAccessIds)
                allAccessIds.Add(id);

            await _cardRepository.AssignCardAccessAsync(dto.CardId, allAccessIds, UsernameFormToken);
        }
    }
}
