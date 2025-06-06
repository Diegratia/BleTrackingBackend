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

namespace BusinessLogic.Services.Implementation
{
    public class FloorplanMaskedAreaService : IFloorplanMaskedAreaService
    {
        private readonly FloorplanMaskedAreaRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FloorplanMaskedAreaService(FloorplanMaskedAreaRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FloorplanMaskedAreaDto> GetByIdAsync(Guid id)
        {
            var area = await _repository.GetByIdAsync(id);
            return area == null ? null : _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task<IEnumerable<FloorplanMaskedAreaDto>> GetAllAsync()
        {
            var areas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<FloorplanMaskedAreaDto>>(areas);
        }

        public async Task<FloorplanMaskedAreaDto> CreateAsync(FloorplanMaskedAreaCreateDto createDto)
        {
            var floor = await _repository.GetFloorByIdAsync(createDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {createDto.FloorId} not found.");

            var area = _mapper.Map<FloorplanMaskedArea>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            area.Id = Guid.NewGuid();
            area.Status = 1;
            area.CreatedBy = username;
            area.CreatedAt = DateTime.UtcNow;
            area.UpdatedBy = username;
            area.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(area);
            return _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto)
        {
            var floor = await _repository.GetFloorByIdAsync(updateDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {updateDto.FloorId} not found.");

            var area = await _repository.GetByIdAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            area.UpdatedBy = username;
            area.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, area);
            await _repository.UpdateAsync(area);
        }

        public async Task DeleteAsync(Guid id)
        {
            var area = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            area.UpdatedBy = username;
            await _repository.SoftDeleteAsync(id);
        }
    }
}