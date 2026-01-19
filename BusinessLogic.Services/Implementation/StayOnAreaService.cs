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
    public class StayOnAreaService : IStayOnAreaService
    {
        private readonly StayOnAreaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public StayOnAreaService(StayOnAreaRepository repository, 
        IMapper mapper, 
        IHttpContextAccessor httpContextAccessor, IAuditEmitter audit)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
        }

        public async Task<StayOnAreaDto> GetByIdAsync(Guid id)
        {
            var onArea = await _repository.GetByIdAsync(id);
            return onArea == null ? null : _mapper.Map<StayOnAreaDto>(onArea);
        }

        public async Task<IEnumerable<StayOnAreaDto>> GetAllAsync()
        {
            var onAreas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<StayOnAreaDto>>(onAreas);
        }

        public async Task<StayOnAreaDto> CreateAsync(StayOnAreaCreateDto createDto)
        {
            var onArea = _mapper.Map<StayOnArea>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            onArea.Id = Guid.NewGuid();
            onArea.Status = 1;
            onArea.CreatedBy = username;
            onArea.CreatedAt = DateTime.UtcNow;
            onArea.UpdatedBy = username;
            onArea.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(onArea);
            await _audit.Created(
                "StayOnArea",
                onArea.Id,
                "Created StayOnArea",
                new { onArea.Name }
            );
            return _mapper.Map<StayOnAreaDto>(onArea);
        }

        public async Task UpdateAsync(Guid id, StayOnAreaUpdateDto updateDto)
        {
            var onArea = await _repository.GetByIdAsync(id);
            if (onArea == null)
                throw new KeyNotFoundException("StayOnArea not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            onArea.UpdatedBy = username;
            onArea.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, onArea);
            await _audit.Updated(
                "StayOnArea",
                onArea.Id,
                "Updated StayOnArea",
                new { onArea.Name }
            );
            await _repository.UpdateAsync(onArea);
        }

        public async Task DeleteAsync(Guid id)
        {
            var onArea = await _repository.GetByIdAsync(id);
            if (onArea == null)
            {
                throw new KeyNotFoundException("StayOnArea not found");
            }
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            onArea.Status = 0;
            onArea.IsActive = 0;
            onArea.UpdatedBy = username;
            onArea.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
            await _audit.Deleted(
                "StayOnArea",
                onArea.Id,
                "Deleted StayOnArea",
                new { onArea.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name"};
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status"};

            var filterService = new GenericDataTableService<StayOnArea, StayOnAreaDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}