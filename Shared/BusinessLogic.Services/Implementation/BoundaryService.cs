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
    public class BoundaryService : IBoundaryService
    {
        private readonly BoundaryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BoundaryService(BoundaryRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BoundaryDto> GetByIdAsync(Guid id)
        {
            var boundary = await _repository.GetByIdAsync(id);
            return boundary == null ? null : _mapper.Map<BoundaryDto>(boundary);
        }

        public async Task<IEnumerable<BoundaryDto>> GetAllAsync()
        {
            var boundarys = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<BoundaryDto>>(boundarys);
        }

        public async Task<BoundaryDto> CreateAsync(BoundaryCreateDto createDto)
        {
            var boundary = _mapper.Map<Boundary>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            boundary.Id = Guid.NewGuid();
            boundary.Status = 1;
            boundary.CreatedBy = username;
            boundary.CreatedAt = DateTime.UtcNow;
            boundary.UpdatedBy = username;
            boundary.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(boundary);
            return _mapper.Map<BoundaryDto>(boundary);
        }

        public async Task UpdateAsync(Guid id, BoundaryUpdateDto updateDto)
        {
            var boundary = await _repository.GetByIdAsync(id);
            if (boundary == null)
                throw new KeyNotFoundException("Boundary not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            boundary.UpdatedBy = username;
            boundary.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, boundary);
            await _repository.UpdateAsync(boundary);
        }

        public async Task DeleteAsync(Guid id)
        {
            var boundary = await _repository.GetByIdAsync(id);
            if (boundary == null)
            {
                throw new KeyNotFoundException("boundary not found");
            }
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            boundary.Status = 0;
            boundary.IsActive = 0;
            boundary.UpdatedBy = username;
            boundary.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name"};
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status"};

            var filterService = new GenericDataTableService<Boundary, BoundaryDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        
        
    }
}