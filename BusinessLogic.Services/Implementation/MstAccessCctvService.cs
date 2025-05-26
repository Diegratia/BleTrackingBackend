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
    public class MstAccessCctvService : IMstAccessCctvService
    {
        private readonly MstAccessCctvRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstAccessCctvService(MstAccessCctvRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstAccessCctvDto> GetByIdAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            return accessCctv == null ? null : _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task<IEnumerable<MstAccessCctvDto>> GetAllAsync()
        {
            var accessCctvs = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstAccessCctvDto>>(accessCctvs);
        }

        public async Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto)
        {
            var accessCctv = _mapper.Map<MstAccessCctv>(createDto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            accessCctv.Id = Guid.NewGuid();
            accessCctv.Status = 1;
            accessCctv.CreatedBy = username;
            accessCctv.CreatedAt = DateTime.UtcNow;
            accessCctv.UpdatedBy = username;
            accessCctv.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(accessCctv);
            return _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto)
        {
            var accessCctv = await _repository.GetByIdAsync(id);
            if (accessCctv == null)
                throw new KeyNotFoundException("Access CCTV not found");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            accessCctv.UpdatedBy = username;
            accessCctv.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, accessCctv);
            await _repository.UpdateAsync(accessCctv);
        }

        public async Task DeleteAsync(Guid id)
        {
            var accessCctv = await _repository.GetByIdAsync(id);    
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            accessCctv.UpdatedBy = username;
            await _repository.SoftDeleteAsync(id);
        }
    }
}