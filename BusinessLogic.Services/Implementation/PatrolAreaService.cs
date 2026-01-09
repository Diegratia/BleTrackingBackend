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
using DataView;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolAreaService : BaseService, IPatrolAreaService
    {
        private readonly PatrolAreaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PatrolAreaService(
            PatrolAreaRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
            ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PatrolAreaDto> GetByIdAsync(Guid id)
        {
            var patrolArea = await _repository.GetByIdAsync(id);
            if (patrolArea == null)
                throw new NotFoundException($"PatrolArea with id {id} not found");
            return patrolArea == null ? null : _mapper.Map<PatrolAreaDto>(patrolArea);
        }

        public async Task<IEnumerable<PatrolAreaDto>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PatrolAreaDto>>(patrolAreas);
        }

        public async Task<PatrolAreaDto> CreateAsync(PatrolAreaCreateDto createDto)
        {
            var patrolArea = _mapper.Map<PatrolArea>(createDto);
            // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            // patrolArea.Id = Guid.NewGuid();
            // patrolArea.CreatedBy = username;
            // patrolArea.CreatedAt = DateTime.UtcNow;
            // patrolArea.UpdatedBy = username;
            // patrolArea.UpdatedAt = DateTime.UtcNow;
            SetCreateAudit(patrolArea);
            await _repository.AddAsync(patrolArea);
            return _mapper.Map<PatrolAreaDto>(patrolArea);
        }

        public async Task<PatrolAreaDto> UpdateAsync(Guid id, PatrolAreaUpdateDto updateDto)
        {
            var patrolArea = await _repository.GetByIdAsync(id);
            if (patrolArea == null)
                throw new NotFoundException($"PatrolArea with id {id} not found");

            // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            // patrolArea.UpdatedBy = username;
            // patrolArea.UpdatedAt = DateTime.UtcNow;
            SetUpdateAudit(patrolArea);
            _mapper.Map(updateDto, patrolArea);
            await _repository.UpdateAsync(patrolArea);
            return _mapper.Map<PatrolAreaDto>(patrolArea);
        }

        public async Task DeleteAsync(Guid id)
        {
            var patrolArea = await _repository.GetByIdAsync(id);
            if (patrolArea == null)
                throw new NotFoundException($"PatrolArea with id {id} not found");
            // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            patrolArea.Status = 0;
            patrolArea.IsActive = 0;
            // patrolArea.UpdatedBy = username;
            // patrolArea.UpdatedAt = DateTime.UtcNow;
            SetDeleteAudit(patrolArea);
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status" };

            var filterService = new GenericDataTableService<PatrolArea, PatrolAreaDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
                public async Task<IEnumerable<PatrolAreaLookUpDto>> GetAllLookUpAsync()
        {
            var patrolareas = await _repository.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<PatrolAreaLookUpDto>>(patrolareas);
        }

        
    }
}