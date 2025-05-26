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
    public class MstDistrictService : IMstDistrictService
    {
        private readonly MstDistrictRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstDistrictService(MstDistrictRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var district = _mapper.Map<MstDistrict>(createDto);
            district.Id = Guid.NewGuid();
            district.CreatedBy = username; 
            district.UpdatedBy = username;
            district.Status = 1;

            var createdDistrict = await _repository.AddAsync(district);
            return _mapper.Map<MstDistrictDto>(createdDistrict);
        }

        public async Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var district = await _repository.GetByIdAsync(id);
            if (district == null)
                throw new KeyNotFoundException("District not found");

            _mapper.Map(updateDto, district);
            district.UpdatedBy = username; 

            await _repository.UpdateAsync(district);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var district = await _repository.GetByIdAsync(id);
            district.UpdatedBy = username;
            await _repository.DeleteAsync(id);
        }
    }
}