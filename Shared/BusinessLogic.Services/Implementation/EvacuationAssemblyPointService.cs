using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class EvacuationAssemblyPointService : BaseService, IEvacuationAssemblyPointService
    {
        private readonly EvacuationAssemblyPointRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public EvacuationAssemblyPointService(
            EvacuationAssemblyPointRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<EvacuationAssemblyPointRead> GetByIdAsync(Guid id)
        {
            var assemblyPoint = await _repository.GetByIdAsync(id);
            if (assemblyPoint == null)
                throw new NotFoundException($"Evacuation Assembly Point with id {id} not found");
            return assemblyPoint;
        }

        public async Task<IEnumerable<EvacuationAssemblyPointRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<EvacuationAssemblyPointRead> CreateAsync(EvacuationAssemblyPointCreateDto createDto)
        {
            // Validate floor ownership
            if (createDto.FloorId.HasValue)
            {
                if (!await _repository.FloorExistsAsync(createDto.FloorId.Value))
                    throw new NotFoundException($"Floor with id {createDto.FloorId} not found");

                var invalidFloorIds = await _repository.CheckInvalidFloorOwnershipAsync(createDto.FloorId.Value, AppId);
                if (invalidFloorIds.Any())
                    throw new UnauthorizedException($"FloorId does not belong to this Application");
            }

            // Validate floorplan ownership
            if (createDto.FloorplanId.HasValue)
            {
                if (!await _repository.FloorplanExistsAsync(createDto.FloorplanId.Value))
                    throw new NotFoundException($"Floorplan with id {createDto.FloorplanId} not found");

                var invalidFloorplanIds = await _repository.CheckInvalidFloorplanOwnershipAsync(createDto.FloorplanId.Value, AppId);
                if (invalidFloorplanIds.Any())
                    throw new UnauthorizedException($"FloorplanId does not belong to this Application");
            }

            // Validate masked area ownership
            if (createDto.FloorplanMaskedAreaId.HasValue)
            {
                if (!await _repository.FloorplanMaskedAreaExistsAsync(createDto.FloorplanMaskedAreaId.Value))
                    throw new NotFoundException($"Floorplan Masked Area with id {createDto.FloorplanMaskedAreaId} not found");

                var invalidIds = await _repository.CheckInvalidFloorplanMaskedAreaOwnershipAsync(createDto.FloorplanMaskedAreaId.Value, AppId);
                if (invalidIds.Any())
                    throw new UnauthorizedException($"FloorplanMaskedAreaId does not belong to this Application");
            }

            var assemblyPoint = _mapper.Map<EvacuationAssemblyPoint>(createDto);
            SetCreateAudit(assemblyPoint);
            assemblyPoint.Status = 1;
            assemblyPoint.ApplicationId = AppId;

            await _repository.AddAsync(assemblyPoint);

            _audit.Created(
                "EvacuationAssemblyPoint",
                assemblyPoint.Id,
                "Created evacuation assembly point",
                new { assemblyPoint.Name }
            );

            return _mapper.Map<EvacuationAssemblyPointRead>(assemblyPoint);
        }

        public async Task UpdateAsync(Guid id, EvacuationAssemblyPointUpdateDto updateDto)
        {
            var assemblyPoint = await _repository.GetByIdEntityAsync(id);
            if (assemblyPoint == null)
                throw new KeyNotFoundException("Evacuation Assembly Point not found");

            // Validate floor ownership
            if (updateDto.FloorId.HasValue && updateDto.FloorId != assemblyPoint.FloorId)
            {
                if (!await _repository.FloorExistsAsync(updateDto.FloorId.Value))
                    throw new NotFoundException($"Floor with id {updateDto.FloorId} not found");

                var invalidFloorIds = await _repository.CheckInvalidFloorOwnershipAsync(updateDto.FloorId.Value, AppId);
                if (invalidFloorIds.Any())
                    throw new UnauthorizedException($"FloorId does not belong to this Application");
            }

            // Validate floorplan ownership
            if (updateDto.FloorplanId.HasValue && updateDto.FloorplanId != assemblyPoint.FloorplanId)
            {
                if (!await _repository.FloorplanExistsAsync(updateDto.FloorplanId.Value))
                    throw new NotFoundException($"Floorplan with id {updateDto.FloorplanId} not found");

                var invalidFloorplanIds = await _repository.CheckInvalidFloorplanOwnershipAsync(updateDto.FloorplanId.Value, AppId);
                if (invalidFloorplanIds.Any())
                    throw new UnauthorizedException($"FloorplanId does not belong to this Application");
            }

            // Validate masked area ownership
            if (updateDto.FloorplanMaskedAreaId.HasValue && updateDto.FloorplanMaskedAreaId != assemblyPoint.FloorplanMaskedAreaId)
            {
                if (!await _repository.FloorplanMaskedAreaExistsAsync(updateDto.FloorplanMaskedAreaId.Value))
                    throw new NotFoundException($"Floorplan Masked Area with id {updateDto.FloorplanMaskedAreaId} not found");

                var invalidIds = await _repository.CheckInvalidFloorplanMaskedAreaOwnershipAsync(updateDto.FloorplanMaskedAreaId.Value, AppId);
                if (invalidIds.Any())
                    throw new UnauthorizedException($"FloorplanMaskedAreaId does not belong to this Application");
            }

            SetUpdateAudit(assemblyPoint);
            _mapper.Map(updateDto, assemblyPoint);
            await _repository.UpdateAsync(assemblyPoint);

            _audit.Updated(
                "EvacuationAssemblyPoint",
                assemblyPoint.Id,
                "Updated evacuation assembly point",
                new { assemblyPoint.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var assemblyPoint = await _repository.GetByIdEntityAsync(id);
            if (assemblyPoint == null)
            {
                throw new KeyNotFoundException("Evacuation Assembly Point not found");
            }

            SetDeleteAudit(assemblyPoint);
            assemblyPoint.Status = 0;

            await _repository.DeleteAsync(id);

            _audit.Deleted(
                "EvacuationAssemblyPoint",
                assemblyPoint.Id,
                "Deleted evacuation assembly point",
                new { assemblyPoint.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, EvacuationAssemblyPointFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        public async Task<List<EvacuationAssemblyPointRead>> GetByFloorplanIdAsync(Guid floorplanId)
        {
            return await _repository.GetByFloorplanIdAsync(floorplanId);
        }
    }
}
