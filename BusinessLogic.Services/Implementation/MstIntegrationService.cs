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
    public class MstIntegrationService : IMstIntegrationService
    {
        private readonly MstIntegrationRepository _repository;
        private readonly IMapper _mapper;
        private  readonly IHttpContextAccessor _httpContextAccessor;

        public MstIntegrationService(MstIntegrationRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MstIntegrationDto> GetByIdAsync(Guid id)
        {
            var integration = await _repository.GetByIdAsync(id);
            return integration == null ? null : _mapper.Map<MstIntegrationDto>(integration);
        }

        public async Task<IEnumerable<MstIntegrationDto>> GetAllAsync()
        {
            var integrations = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstIntegrationDto>>(integrations);
        }

        public async Task<MstIntegrationDto> CreateAsync(MstIntegrationCreateDto createDto)
        {
            // Validasi BrandId
            var brand = await _repository.GetBrandByIdAsync(createDto.BrandId);
            if (brand == null)
                throw new ArgumentException($"Brand with ID {createDto.BrandId} not found.");

            // Validasi ApplicationId
            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var integration = _mapper.Map<MstIntegration>(createDto);
            integration.Id = Guid.NewGuid();
            integration.Status = 1;
            integration.CreatedBy = username;
            integration.CreatedAt = DateTime.UtcNow;
            integration.UpdatedBy = username;
            integration.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(integration);
            return _mapper.Map<MstIntegrationDto>(integration);
        }

        public async Task UpdateAsync(Guid id, MstIntegrationUpdateDto updateDto)
        {
            var integration = await _repository.GetByIdAsync(id);
            if (integration == null)
                throw new KeyNotFoundException("Integration not found");

            // Validasi BrandId jika berubah
            if (integration.BrandId != updateDto.BrandId)
            {
                var brand = await _repository.GetBrandByIdAsync(updateDto.BrandId);
                if (brand == null)
                    throw new ArgumentException($"Brand with ID {updateDto.BrandId} not found.");
                integration.BrandId = updateDto.BrandId;
            }

            // Validasi ApplicationId jika berubah
            if (integration.ApplicationId != updateDto.ApplicationId)
            {
                var application = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
                integration.ApplicationId = updateDto.ApplicationId;
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            integration.UpdatedBy = username;
            integration.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(updateDto, integration);

            await _repository.UpdateAsync(integration);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var integration = await _repository.GetByIdAsync(id);
            integration.UpdatedBy = username;
            await _repository.SoftDeleteAsync(id);
        }
    }
}