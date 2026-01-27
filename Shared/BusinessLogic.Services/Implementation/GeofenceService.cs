using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

namespace BusinessLogic.Services.Implementation
{
    public class GeofenceService : IGeofenceService
    {
        private readonly GeofenceRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public GeofenceService(GeofenceRepository repository, 
        IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuditEmitter audit)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
        }

        public async Task<GeofenceDto> GetByIdAsync(Guid id)
        {
            var geofence = await _repository.GetByIdAsync(id);
            return geofence == null ? null : _mapper.Map<GeofenceDto>(geofence);
        }

        public async Task<IEnumerable<GeofenceDto>> GetAllAsync()
        {
            var geofences = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<GeofenceDto>>(geofences);
        }

        public async Task<GeofenceDto> CreateAsync(GeofenceCreateDto createDto)
        {
            var geofence = _mapper.Map<Geofence>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            geofence.Id = Guid.NewGuid();
            geofence.Status = 1;
            geofence.CreatedBy = username;
            geofence.CreatedAt = DateTime.UtcNow;
            geofence.UpdatedBy = username;
            geofence.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(geofence);
            await _audit.Created(
                "Geofence",
                geofence.Id,
                "Created geofence",
                new { geofence.Name }
            );
            return _mapper.Map<GeofenceDto>(geofence);
        }

        public async Task UpdateAsync(Guid id, GeofenceUpdateDto updateDto)
        {
            var geofence = await _repository.GetByIdAsync(id);
            if (geofence == null)
                throw new KeyNotFoundException("Geofence not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            geofence.UpdatedBy = username;
            geofence.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, geofence);
            await _repository.UpdateAsync(geofence);
            await _audit.Updated(
                "Geofence",
                geofence.Id,
                "Updated geofence",
                new { geofence.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var geofence = await _repository.GetByIdAsync(id);
            if (geofence == null)
            {
                throw new KeyNotFoundException("Geofence not found");
            }
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            geofence.Status = 0;
            geofence.IsActive = 0;
            geofence.UpdatedBy = username;
            geofence.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
            await _audit.Deleted(
                "Geofence",
                geofence.Id,
                "Deleted geofence",
                new { geofence.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name"};
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status"};

            var filterService = new GenericDataTableService<Geofence, GeofenceDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}