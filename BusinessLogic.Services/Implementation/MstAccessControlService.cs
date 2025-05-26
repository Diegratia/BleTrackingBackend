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
    public class MstAccessControlService : IMstAccessControlService
    {
        private readonly MstAccessControlRepository _repository;
        private readonly IMapper _mapper;

        public MstAccessControlService(MstAccessControlRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MstAccessControlDto> GetByIdAsync(Guid id)
        {
            var accessControl = await _repository.GetByIdAsync(id);
            return accessControl == null ? null : _mapper.Map<MstAccessControlDto>(accessControl);
        }

        public async Task<IEnumerable<MstAccessControlDto>> GetAllAsync()
        {
            var accessControls = await _repository
            .GetAllAsync();
            return _mapper.Map<IEnumerable<MstAccessControlDto>>(accessControls);
        }

        public async Task<MstAccessControlDto> CreateAsync(MstAccessControlCreateDto createDto)
        {
            var accessControl = _mapper.Map<MstAccessControl>(createDto);
            
            accessControl.Id = Guid.NewGuid();
            accessControl.Status = 1;
            accessControl.CreatedBy = "System";
            accessControl.CreatedAt = DateTime.UtcNow;
            accessControl.UpdatedBy = "System";
            accessControl.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(accessControl);
            return _mapper.Map<MstAccessControlDto>(accessControl);
        }

        public async Task UpdateAsync(Guid id, MstAccessControlUpdateDto updateDto)
        {
            var accessControl = await _repository.GetByIdAsync(id);
            if (accessControl == null)
                throw new KeyNotFoundException("Access Control not found");

            accessControl.UpdatedBy = "System";
            accessControl.UpdatedAt = DateTime.UtcNow;

            _mapper.Map(updateDto, accessControl);
            await _repository.UpdateAsync(accessControl);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }
    }
}