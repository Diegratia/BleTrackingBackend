using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class MstBuildingService : IMstBuildingService
    {
        private readonly MstBuildingRepository _repository;
        private readonly IMapper _mapper;

        public MstBuildingService(MstBuildingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
            var building = _mapper.Map<MstBuilding>(createDto);
            building.Id = Guid.NewGuid();
            building.CreatedBy = "System"; 
            building.UpdatedBy = "System";
            building.Status = 1;

            var createdBuilding = await _repository.AddAsync(building);
            return _mapper.Map<MstBuildingDto>(createdBuilding);
        }

        public async Task UpdateAsync(Guid id, MstBuildingUpdateDto updateDto)
        {
            var building = await _repository.GetByIdAsync(id);
            if (building == null)
                throw new KeyNotFoundException("Building not found");

            _mapper.Map(updateDto, building);
            building.UpdatedBy = "System";

            await _repository.UpdateAsync(building);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}