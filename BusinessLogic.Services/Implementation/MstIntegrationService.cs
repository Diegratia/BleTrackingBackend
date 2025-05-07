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
    public class MstIntegrationService : IMstIntegrationService
    {
        private readonly MstIntegrationRepository _repository;
        private readonly IMapper _mapper;

        public MstIntegrationService(MstIntegrationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
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

            var integration = _mapper.Map<MstIntegration>(createDto);
            integration.Id = Guid.NewGuid();
            integration.Status = 1;
            integration.CreatedBy = "System";
            integration.CreatedAt = DateTime.UtcNow;
            integration.UpdatedBy = "System";
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

            integration.UpdatedBy = "System";
            integration.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(updateDto, integration);

            await _repository.UpdateAsync(integration);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }
    }
}