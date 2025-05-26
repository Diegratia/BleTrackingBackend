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
    public class MstOrganizationService : IMstOrganizationService
    {
        private readonly MstOrganizationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MstOrganizationService(MstOrganizationRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<MstOrganizationDto>> GetAllOrganizationsAsync()
        {
            var organizations = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<MstOrganizationDto>>(organizations);
        }

        public async Task<MstOrganizationDto> GetOrganizationByIdAsync(Guid id)
        {
            var organization = await _repository.GetByIdAsync(id);
            return organization == null ? null : _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task<MstOrganizationDto> CreateOrganizationAsync(MstOrganizationCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var organization = _mapper.Map<MstOrganization>(dto);
            organization.Id = Guid.NewGuid();
            organization.Status = 1;
            organization.CreatedBy = username; 
            organization.CreatedAt = DateTime.UtcNow;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(organization);
            return _mapper.Map<MstOrganizationDto>(organization);
        }

        public async Task UpdateOrganizationAsync(Guid id, MstOrganizationUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var organization = await _repository.GetByIdAsync(id);
            if (organization == null || organization.Status == 0)
                throw new KeyNotFoundException($"Organization with ID {id} not found or has been deleted.");

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, organization);

            await _repository.UpdateAsync(organization);
        }

        public async Task DeleteOrganizationAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var organization = await _repository.GetByIdAsync(id);
            if (organization == null || organization.Status == 0)
                throw new KeyNotFoundException($"Organization with ID {id} not found or already deleted.");

            organization.Status = 0;
            organization.UpdatedBy = username;
            organization.UpdatedAt = DateTime.UtcNow;

            await _repository.DeleteAsync(organization);
        }
    }
}
