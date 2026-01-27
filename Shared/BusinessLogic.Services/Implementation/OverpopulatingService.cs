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
    public class OverpopulatingService : IOverpopulatingService
    {
        private readonly OverpopulatingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public OverpopulatingService(OverpopulatingRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, IAuditEmitter audit)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
        }

        public async Task<OverpopulatingDto> GetByIdAsync(Guid id)
        {
            var overpopulating = await _repository.GetByIdAsync(id);
            return overpopulating == null ? null : _mapper.Map<OverpopulatingDto>(overpopulating);
        }

        public async Task<IEnumerable<OverpopulatingDto>> GetAllAsync()
        {
            var overpopulatings = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<OverpopulatingDto>>(overpopulatings);
        }

        public async Task<OverpopulatingDto> CreateAsync(OverpopulatingCreateDto createDto)
        {
            var overpopulating = _mapper.Map<Overpopulating>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            overpopulating.Id = Guid.NewGuid();
            overpopulating.Status = 1;
            overpopulating.CreatedBy = username;
            overpopulating.CreatedAt = DateTime.UtcNow;
            overpopulating.UpdatedBy = username;
            overpopulating.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(overpopulating);
            await _audit.Created(
                "Overpopulating",
                overpopulating.Id,
                "Created Overpopulating",
                new { overpopulating.Name }
            );
            return _mapper.Map<OverpopulatingDto>(overpopulating);
        }

        public async Task UpdateAsync(Guid id, OverpopulatingUpdateDto updateDto)
        {
            var overpopulating = await _repository.GetByIdAsync(id);
            if (overpopulating == null)
                throw new KeyNotFoundException("Overpopulating not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            overpopulating.UpdatedBy = username;
            overpopulating.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, overpopulating);
            await _repository.UpdateAsync(overpopulating);
            await _audit.Updated(
                "Overpopulating",
                overpopulating.Id,
                "Updated Overpopulating",
                new { overpopulating.Name }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var overpopulating = await _repository.GetByIdAsync(id);
            if (overpopulating == null)
            {
                throw new KeyNotFoundException("Overpopulating not found");
            }
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            overpopulating.UpdatedBy = username;
            overpopulating.UpdatedAt = DateTime.UtcNow;
            overpopulating.Status = 0;
            overpopulating.IsActive = 0;
            await _repository.DeleteAsync(id);
            await _audit.Deleted(
                "Overpopulating",
                overpopulating.Id,
                "Deleted Overpopulating",
                new { overpopulating.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name"};
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status"};

            var filterService = new GenericDataTableService<Overpopulating, OverpopulatingDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}