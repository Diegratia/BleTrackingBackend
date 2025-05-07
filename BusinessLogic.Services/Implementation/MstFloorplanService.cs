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
    public class MstFloorplanService : IMstFloorplanService
    {
        private readonly MstFloorplanRepository _repository;
        private readonly IMapper _mapper;

        public MstFloorplanService(MstFloorplanRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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
            var floorplan = _mapper.Map<MstFloorplan>(createDto);
            floorplan.Id = Guid.NewGuid();

            var createdFloorplan = await _repository.AddAsync(floorplan);
            return _mapper.Map<MstFloorplanDto>(createdFloorplan);
        }

        public async Task UpdateAsync(Guid id, MstFloorplanUpdateDto updateDto)
        {
            var floorplan = await _repository.GetByIdAsync(id);
            if (floorplan == null)
                throw new KeyNotFoundException("Floorplan not found");

            _mapper.Map(updateDto, floorplan);
            await _repository.UpdateAsync(floorplan);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}