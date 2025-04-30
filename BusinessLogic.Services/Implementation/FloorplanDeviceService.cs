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
    public class FloorplanDeviceService : IFloorplanDeviceService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public FloorplanDeviceService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FloorplanDeviceDto> GetByIdAsync(Guid id)
        {
            var device = await _context.FloorplanDevices
                .Include(fd => fd.Floorplan)
                .Include(fd => fd.AccessCctv)
                .Include(fd => fd.Reader)
                .Include(fd => fd.AccessControl)
                .Include(fd => fd.FloorplanMaskedArea)
                .Include(fd => fd.Application)
                .FirstOrDefaultAsync(fd => fd.Id == id && fd.Status != 0);

            return device == null ? null : _mapper.Map<FloorplanDeviceDto>(device);
        }

        public async Task<IEnumerable<FloorplanDeviceDto>> GetAllAsync()
        {
            var devices = await _context.FloorplanDevices
                .Include(fd => fd.Floorplan)
                .Include(fd => fd.AccessCctv)
                .Include(fd => fd.Reader)
                .Include(fd => fd.AccessControl)
                .Include(fd => fd.FloorplanMaskedArea)
                .Include(fd => fd.Application)
                .Where(fd => fd.Status != 0)
                .ToListAsync();

            return _mapper.Map<IEnumerable<FloorplanDeviceDto>>(devices);
        }

        public async Task<FloorplanDeviceDto> CreateAsync(FloorplanDeviceCreateDto dto)
        {
            // Validasi semua foreign key
            var floorplan = await _context.MstFloorplans.FirstOrDefaultAsync(f => f.Id == dto.FloorplanId);
            if (floorplan == null)
                throw new ArgumentException($"Floorplan with ID {dto.FloorplanId} not found.");

            var accessCctv = await _context.MstAccessCctvs.FirstOrDefaultAsync(c => c.Id == dto.AccessCctvId);
            if (accessCctv == null)
                throw new ArgumentException($"AccessCctv with ID {dto.AccessCctvId} not found.");

            var reader = await _context.MstBleReaders.FirstOrDefaultAsync(r => r.Id == dto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {dto.ReaderId} not found.");

            var accessControl = await _context.MstAccessControls.FirstOrDefaultAsync(ac => ac.Id == dto.AccessControlId);
            if (accessControl == null)
                throw new ArgumentException($"AccessControl with ID {dto.AccessControlId} not found.");

            var floorplanMaskedArea = await _context.FloorplanMaskedAreas.FirstOrDefaultAsync(fma => fma.Id == dto.FloorplanMaskedAreaId);
            if (floorplanMaskedArea == null)
                throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");

            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");

            var device = _mapper.Map<FloorplanDevice>(dto);
            device.Id = Guid.NewGuid();
            device.Status = 1;
            device.CreatedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;
            device.CreatedBy ??= "";
            device.UpdatedBy ??= "";

            _context.FloorplanDevices.Add(device);
            await _context.SaveChangesAsync();

            // Kembalikan FloorplanDeviceDto tanpa relasi
            return _mapper.Map<FloorplanDeviceDto>(device);
        }

        public async Task UpdateAsync(Guid id, FloorplanDeviceUpdateDto dto)
        {
            var device = await _context.FloorplanDevices.FindAsync(id);
            if (device == null || device.Status == 0)
                throw new KeyNotFoundException("FloorplanDevice not found");

            // Validasi foreign key jika berubah
            if (device.FloorplanId != dto.FloorplanId)
            {
                var floorplan = await _context.MstFloorplans.FirstOrDefaultAsync(f => f.Id == dto.FloorplanId);
                if (floorplan == null)
                    throw new ArgumentException($"Floorplan with ID {dto.FloorplanId} not found.");
                device.FloorplanId = dto.FloorplanId;
            }

            if (device.AccessCctvId != dto.AccessCctvId)
            {
                var accessCctv = await _context.MstAccessCctvs.FirstOrDefaultAsync(c => c.Id == dto.AccessCctvId);
                if (accessCctv == null)
                    throw new ArgumentException($"AccessCctv with ID {dto.AccessCctvId} not found.");
                device.AccessCctvId = dto.AccessCctvId;
            }

            if (device.ReaderId != dto.ReaderId)
            {
                var reader = await _context.MstBleReaders.FirstOrDefaultAsync(r => r.Id == dto.ReaderId);
                if (reader == null)
                    throw new ArgumentException($"Reader with ID {dto.ReaderId} not found.");
                device.ReaderId = dto.ReaderId;
            }

            if (device.AccessControlId != dto.AccessControlId)
            {
                var accessControl = await _context.MstAccessControls.FirstOrDefaultAsync(ac => ac.Id == dto.AccessControlId);
                if (accessControl == null)
                    throw new ArgumentException($"AccessControl with ID {dto.AccessControlId} not found.");
                device.AccessControlId = dto.AccessControlId;
            }

            if (device.FloorplanMaskedAreaId != dto.FloorplanMaskedAreaId)
            {
                var floorplanMaskedArea = await _context.FloorplanMaskedAreas.FirstOrDefaultAsync(fma => fma.Id == dto.FloorplanMaskedAreaId);
                if (floorplanMaskedArea == null)
                    throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");
                device.FloorplanMaskedAreaId = dto.FloorplanMaskedAreaId;
            }

            if (device.ApplicationId != dto.ApplicationId)
            {
                var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");
                device.ApplicationId = dto.ApplicationId;
            }

            device.UpdatedBy ??= "";
            device.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, device);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var device = await _context.FloorplanDevices.FindAsync(id);
            if (device == null || device.Status == 0)
                throw new KeyNotFoundException("FloorplanDevice not found");

            device.Status = 0; // Soft delete
            await _context.SaveChangesAsync();
        }
    }
}