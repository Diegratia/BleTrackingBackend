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
    public class BleReaderNodeService : IBleReaderNodeService
    {
        private readonly BleReaderNodeRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BleReaderNodeService(
            BleReaderNodeRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BleReaderNodeDto> GetByIdAsync(Guid id)
        {
            var bleReaderNode = await _repository.GetByIdAsync(id);
            return bleReaderNode == null ? null : _mapper.Map<BleReaderNodeDto>(bleReaderNode);
        }

        public async Task<IEnumerable<BleReaderNodeDto>> GetAllAsync()
        {
            var bleReaderNodes = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<BleReaderNodeDto>>(bleReaderNodes);
        }

        public async Task<BleReaderNodeDto> CreateAsync(BleReaderNodeCreateDto createDto)
        {
            // Validasi DTO
            var validationContext = new ValidationContext(createDto);
            Validator.ValidateObject(createDto, validationContext, true);

            // Validasi ReaderId
            var reader = await _repository.GetReaderByIdAsync(createDto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {createDto.ReaderId} not found.");

            // Validasi ApplicationId
            var application = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var bleReaderNode = _mapper.Map<BleReaderNode>(createDto);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            bleReaderNode.CreatedBy = username;
            bleReaderNode.CreatedAt = DateTime.UtcNow;
            bleReaderNode.UpdatedBy = username;
            bleReaderNode.UpdatedAt = DateTime.UtcNow;

            await _repository.AddAsync(bleReaderNode);
            return _mapper.Map<BleReaderNodeDto>(bleReaderNode);
        }

        public async Task UpdateAsync(Guid id, BleReaderNodeUpdateDto updateDto)
        {
            // Validasi DTO
            var validationContext = new ValidationContext(updateDto);
            Validator.ValidateObject(updateDto, validationContext, true);

            // Validasi ReaderId
            var reader = await _repository.GetReaderByIdAsync(updateDto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {updateDto.ReaderId} not found.");

            // Validasi ApplicationId
            var application = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

            var bleReaderNode = await _repository.GetByIdAsync(id);
            if (bleReaderNode == null)
                throw new KeyNotFoundException("BleReaderNode not found");

            _mapper.Map(updateDto, bleReaderNode);
            bleReaderNode.UpdatedBy = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            bleReaderNode.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(bleReaderNode);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}