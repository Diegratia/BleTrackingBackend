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
    public class MstFloorplanService : IMstFloorplanService
    {
        private readonly MstFloorplanRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstFloorplanService(MstFloorplanRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstFloorplanDto> GetByIdAsync(Guid id)
        {
            var floorplan = await _repository.GetByIdAsync(id);
            return floorplan == null ? null : _mapper.Map<MstFloorplanDto>(floorplan);
        }

        public async Task<IEnumerable<MstFloorplanDto>> GetAllAsync()
        {
            var floorplans = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstFloorplanDto>>(floorplans);
        }

        public async Task<MstFloorplanDto> CreateAsync(MstFloorplanCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floorplan = _mapper.Map<MstFloorplan>(createDto);
            floorplan.Id = Guid.NewGuid();
                floorplan.CreatedBy = username;
                floorplan.UpdatedBy = username;
                floorplan.CreatedAt = DateTime.UtcNow;
                floorplan.UpdatedAt = DateTime.UtcNow;

            var createdFloorplan = await _repository.AddAsync(floorplan);
            return _mapper.Map<MstFloorplanDto>(createdFloorplan);
        }

        public async Task UpdateAsync(Guid id, MstFloorplanUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floorplan = await _repository.GetByIdAsync(id);
            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found");

            floorplan.UpdatedBy = username;
            floorplan.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, floorplan);
            await _repository.UpdateAsync(floorplan);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var floorplan = await _repository.GetByIdAsync(id);
            floorplan.UpdatedBy = username;
            await _repository.DeleteAsync(id);
        }
    }
}