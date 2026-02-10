using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using ClosedXML.Excel;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class GeofenceService : BaseService, IGeofenceService
    {
        private readonly GeofenceRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public GeofenceService(GeofenceRepository repository,
        IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<GeofenceRead> GetByIdAsync(Guid id)
        {
            var geofence = await _repository.GetByIdAsync(id);
            if (geofence == null)
                throw new NotFoundException($"Geofence with id {id} not found");
            return geofence;
        }

        public async Task<IEnumerable<GeofenceRead>> GetAllAsync()
        {
            var geofences = await _repository.GetAllAsync();
            return geofences;
        }

        public async Task<GeofenceRead> CreateAsync(GeofenceCreateDto createDto)
        {
            if (createDto.FloorId.HasValue)
            {
                if (!await _repository.FloorExistsAsync(createDto.FloorId.Value))
                    throw new NotFoundException($"Floor with id {createDto.FloorId} not found");

                var invalidFloorId = await _repository.CheckInvalidFloorOwnershipAsync(createDto.FloorId.Value, AppId);
                if (invalidFloorId.Any())
                    throw new UnauthorizedException($"FloorId does not belong to this Application: {string.Join(", ", invalidFloorId)}");
            }

            if (createDto.FloorplanId.HasValue)
            {
                if (!await _repository.FloorplanExistsAsync(createDto.FloorplanId.Value))
                    throw new NotFoundException($"Floorplan with id {createDto.FloorplanId} not found");

                var invalidFloorplanId = await _repository.CheckInvalidFloorplanOwnershipAsync(createDto.FloorplanId.Value, AppId);
                if (invalidFloorplanId.Any())
                {
                    throw new UnauthorizedException(
                        $"FloorplanId does not belong to this Application: {string.Join(", ", invalidFloorplanId)}"
                    );
                }
            }

            var geofence = _mapper.Map<Geofence>(createDto);
            SetCreateAudit(geofence);
            geofence.Status = 1;

            await _repository.AddAsync(geofence);
             _audit.Created(
                "Geofence",
                geofence.Id,
                "Created geofence",
                new { geofence.Name }
            );
            return _mapper.Map<GeofenceRead>(geofence);
        }

        public async Task UpdateAsync(Guid id, GeofenceUpdateDto updateDto)
        {
            var geofence = await _repository.GetByIdEntityAsync(id);
            if (geofence == null)
                throw new KeyNotFoundException("Geofence not found");

            if (updateDto.FloorId.HasValue && updateDto.FloorId != geofence.FloorId)
            {
                if (!await _repository.FloorExistsAsync(updateDto.FloorId.Value))
                    throw new NotFoundException($"Floor with id {updateDto.FloorId} not found");

                var invalidFloorId = await _repository.CheckInvalidFloorOwnershipAsync(updateDto.FloorId.Value, AppId);
                if (invalidFloorId.Any())
                    throw new UnauthorizedException($"FloorId does not belong to this Application: {string.Join(", ", invalidFloorId)}");
            }

            if (updateDto.FloorplanId.HasValue && updateDto.FloorplanId != geofence.FloorplanId)
            {
                if (!await _repository.FloorplanExistsAsync(updateDto.FloorplanId.Value))
                    throw new NotFoundException($"Floorplan with id {updateDto.FloorplanId} not found");

                var invalidFloorplanId = await _repository.CheckInvalidFloorplanOwnershipAsync(updateDto.FloorplanId.Value, AppId);
                if (invalidFloorplanId.Any())
                {
                    throw new UnauthorizedException(
                        $"FloorplanId does not belong to this Application: {string.Join(", ", invalidFloorplanId)}"
                    );
                }
            }

            SetUpdateAudit(geofence);
            _mapper.Map(updateDto, geofence);
            await _repository.UpdateAsync(geofence);
             _audit.Updated(
                "Geofence",
                geofence.Id,
                "Updated geofence",
                new { geofence.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var geofence = await _repository.GetByIdEntityAsync(id);
            if (geofence == null)
            {
                throw new KeyNotFoundException("Geofence not found");
            }

            SetDeleteAudit(geofence);
            geofence.IsActive = 0; // Keeping manual set for IsActive as not in SetDeleteAudit for BaseModelWithTime? BaseService.SetDeleteAudit usually handles Status = 0.
            // But checking BaseService SetDeleteAudit(BaseModelWithTime) only updates UpdatedBy/At. It does NOT set Status=0 in the code I viewed?
            // Wait, I checked BaseService.cs in step 255.
            // protected void SetDeleteAudit(BaseModelWithTime entity) { // entity.Status = 0; entity.UpdatedBy... }
            // The Status=0 line is commented out in BaseService! " // entity.Status = 0;"
            // So I MUST set Status = 0 manually here if BaseService doesn't do it.
            geofence.Status = 0;

            // Re-checking BaseService content...
            // Line 113: // entity.Status = 1; (in CreateAudit)
            // Line 113: // entity.Status = 0; (in DeleteAudit)
            // It seems BaseService DOES NOT set Status. So I must set it.

            await _repository.DeleteAsync(id);
             _audit.Deleted(
                "Geofence",
                geofence.Id,
                "Deleted geofence",
                new { geofence.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, GeofenceFilter filter)
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

    }
}