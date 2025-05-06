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
    public class MstBrandService : IMstBrandService
    {
        private readonly MstBrandRepository _repository;
        private readonly IMapper _mapper;

        public MstBrandService(MstBrandRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MstBrandDto> GetByIdAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            return brand == null ? null : _mapper.Map<MstBrandDto>(brand);
        }

        public async Task<IEnumerable<MstBrandDto>> GetAllAsync()
        
        {
            var brands = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstBrandDto>>(brands);
        }

        public async Task<MstBrandDto> CreateAsync(MstBrandCreateDto createDto)
        {
            var brand = _mapper.Map<MstBrand>(createDto);
            brand.Status = 1;

            await _repository.AddAsync(brand);
            return _mapper.Map<MstBrandDto>(brand);
        }

        public async Task UpdateAsync(Guid id, MstBrandUpdateDto updateDto)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");
            _mapper.Map(updateDto, brand);

            await _repository.UpdateAsync(brand);
        }

        public async Task DeleteAsync(Guid id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null)
                throw new KeyNotFoundException("Brand not found");

            brand.Status = 0;
            await _repository.DeleteAsync(brand);
        }
    }
}