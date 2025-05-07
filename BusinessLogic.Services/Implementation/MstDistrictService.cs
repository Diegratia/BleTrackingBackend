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
    public class MstDistrictService : IMstDistrictService
    {
        private readonly MstDistrictRepository _repository;
        private readonly IMapper _mapper;

        public MstDistrictService(MstDistrictRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MstDistrictDto> GetByIdAsync(Guid id)
        {
            var district = await _repository.GetByIdAsync(id);
            return district == null ? null : _mapper.Map<MstDistrictDto>(district);
        }

        public async Task<IEnumerable<MstDistrictDto>> GetAllAsync()
        {
            var districts = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstDistrictDto>>(districts);
        }

        public async Task<MstDistrictDto> CreateAsync(MstDistrictCreateDto createDto)
        {
            var district = _mapper.Map<MstDistrict>(createDto);
            district.Id = Guid.NewGuid();
            district.CreatedBy = "System"; 
            district.UpdatedBy = "System";
            district.Status = 1;

            var createdDistrict = await _repository.AddAsync(district);
            return _mapper.Map<MstDistrictDto>(createdDistrict);
        }

        public async Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto)
        {
            var district = await _repository.GetByIdAsync(id);
            if (district == null)
                throw new KeyNotFoundException("District not found");

            _mapper.Map(updateDto, district);
            district.UpdatedBy = "System"; 

            await _repository.UpdateAsync(district);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}