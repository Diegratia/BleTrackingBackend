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
    public class MstBuildingService : IMstBuildingService
    {
        private readonly MstBuildingRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstBuildingService(MstBuildingRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor; 
        }

        public async Task<MstBuildingDto> GetByIdAsync(Guid id)
        {
            var building = await _repository.GetByIdAsync(id);
            return building == null ? null : _mapper.Map<MstBuildingDto>(building);
        }

        public async Task<IEnumerable<MstBuildingDto>> GetAllAsync()
        {
            var buildings = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBuildingDto>>(buildings);
        }

        public async Task<MstBuildingDto> CreateAsync(MstBuildingCreateDto createDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var building = _mapper.Map<MstBuilding>(createDto);
            building.Id = Guid.NewGuid();
            building.CreatedBy = username; 
            building.UpdatedBy = username;
            building.Status = 1;

            var createdBuilding = await _repository.AddAsync(building);
            return _mapper.Map<MstBuildingDto>(createdBuilding);
        }

        public async Task UpdateAsync(Guid id, MstBuildingUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var building = await _repository.GetByIdAsync(id);
            if (building == null)
                throw new KeyNotFoundException("Building not found");

            _mapper.Map(updateDto, building);
            building.UpdatedBy = username;

            await _repository.UpdateAsync(building);
        }

        public async Task DeleteAsync(Guid id)
        {
            var building = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            building.UpdatedBy = username;
            await _repository.DeleteAsync(id);
        }
    }
}