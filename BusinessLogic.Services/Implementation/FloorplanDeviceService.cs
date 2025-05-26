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
    public class FloorplanDeviceService : IFloorplanDeviceService
    {
        private readonly FloorplanDeviceRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FloorplanDeviceService(FloorplanDeviceRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FloorplanDeviceDto> GetByIdAsync(Guid id)
        {
            var device = await _repository.GetByIdAsync(id);
            return device == null ? null : _mapper.Map<FloorplanDeviceDto>(device);
        }

        public async Task<IEnumerable<FloorplanDeviceDto>> GetAllAsync()
        {
            var devices = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<FloorplanDeviceDto>>(devices);
        }

        public async Task<FloorplanDeviceDto> CreateAsync(FloorplanDeviceCreateDto dto)
        {
            // Validasi semua foreign key
            var floorplan = await _repository.GetFloorplanByIdAsync(dto.FloorplanId);
            if (floorplan == null)
                throw new ArgumentException($"Floorplan with ID {dto.FloorplanId} not found.");

            var accessCctv = await _repository.GetAccessCctvByIdAsync(dto.AccessCctvId);
            if (accessCctv == null)
                throw new ArgumentException($"AccessCctv with ID {dto.AccessCctvId} not found.");

            var reader = await _repository.GetReaderByIdAsync(dto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {dto.ReaderId} not found.");

            var accessControl = await _repository.GetAccessControlByIdAsync(dto.AccessControlId);
            if (accessControl == null)
                throw new ArgumentException($"AccessControl with ID {dto.AccessControlId} not found.");

            var floorplanMaskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(dto.FloorplanMaskedAreaId);
            if (floorplanMaskedArea == null)
                throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");

            var application = await _repository.GetApplicationByIdAsync(dto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");

            var device = _mapper.Map<FloorplanDevice>(dto);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        
            device.CreatedBy = username;
            device.UpdatedBy = username;

            await _repository.AddAsync(device);
            return _mapper.Map<FloorplanDeviceDto>(device);
        }

        public async Task UpdateAsync(Guid id, FloorplanDeviceUpdateDto dto)
        {
            var device = await _repository.GetByIdAsync(id);
            if (device == null)
                throw new KeyNotFoundException("FloorplanDevice not found");

            // Validasi foreign key jika berubah
            if (device.FloorplanId != dto.FloorplanId)
            {
                var floorplan = await _repository.GetFloorplanByIdAsync(dto.FloorplanId);
                if (floorplan == null)
                    throw new ArgumentException($"Floorplan with ID {dto.FloorplanId} not found.");
                device.FloorplanId = dto.FloorplanId;
            }

            if (device.AccessCctvId != dto.AccessCctvId)
            {
                var accessCctv = await _repository.GetAccessCctvByIdAsync(dto.AccessCctvId);
                if (accessCctv == null)
                    throw new ArgumentException($"AccessCctv with ID {dto.AccessCctvId} not found.");
                device.AccessCctvId = dto.AccessCctvId;
            }

            if (device.ReaderId != dto.ReaderId)
            {
                var reader = await _repository.GetReaderByIdAsync(dto.ReaderId);
                if (reader == null)
                    throw new ArgumentException($"Reader with ID {dto.ReaderId} not found.");
                device.ReaderId = dto.ReaderId;
            }

            if (device.AccessControlId != dto.AccessControlId)
            {
                var accessControl = await _repository.GetAccessControlByIdAsync(dto.AccessControlId);
                if (accessControl == null)
                    throw new ArgumentException($"AccessControl with ID {dto.AccessControlId} not found.");
                device.AccessControlId = dto.AccessControlId;
            }

            if (device.FloorplanMaskedAreaId != dto.FloorplanMaskedAreaId)
            {
                var floorplanMaskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(dto.FloorplanMaskedAreaId);
                if (floorplanMaskedArea == null)
                    throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");
                device.FloorplanMaskedAreaId = dto.FloorplanMaskedAreaId;
            }

            if (device.ApplicationId != dto.ApplicationId)
            {
                var application = await _repository.GetApplicationByIdAsync(dto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");
                device.ApplicationId = dto.ApplicationId;
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

            device.UpdatedBy = username;
           
            _mapper.Map(dto, device);

            await _repository.UpdateAsync(device);
        }

        public async Task DeleteAsync(Guid id)
        {
            var device = await _repository.GetByIdAsync(id);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            device.UpdatedBy = username;

            await _repository.SoftDeleteAsync(id);
        }
    }
}