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
    public class MstFloorplanService : IMstFloorplanService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstFloorplanService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstFloorplanDto> GetByIdAsync(Guid id)
        {
            var floorplan = await _context.MstFloorplans
                .Include(f => f.Floor) // Memuat Floor untuk Get response
                .FirstOrDefaultAsync(f => f.Id == id && f.Status != 0);

            return floorplan == null ? null : _mapper.Map<MstFloorplanDto>(floorplan);
        }

        public async Task<IEnumerable<MstFloorplanDto>> GetAllAsync()
        {
            var floorplans = await _context.MstFloorplans
                .Include(f => f.Floor) // Memuat Floor untuk Get response
                .Where(f => f.Status != 0)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MstFloorplanDto>>(floorplans);
        }

        public async Task<MstFloorplanDto> CreateAsync(MstFloorplanCreateDto dto)
        {
            // Validasi FloorId
            var floor = await _context.MstFloors.FirstOrDefaultAsync(f => f.Id == dto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {dto.FloorId} not found.");

            // Validasi ApplicationId
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");

            var floorplan = _mapper.Map<MstFloorplan>(dto);
            floorplan.Id = Guid.NewGuid();
            floorplan.Status = 1;
            floorplan.CreatedAt = DateTime.UtcNow;
            floorplan.UpdatedAt = DateTime.UtcNow;
            floorplan.CreatedBy ??= "";
            floorplan.UpdatedBy ??= "";

            _context.MstFloorplans.Add(floorplan);
            await _context.SaveChangesAsync();

            // Kembalikan MstFloorplanDto tanpa relasi
            return _mapper.Map<MstFloorplanDto>(floorplan);
        }

        public async Task UpdateAsync(Guid Id, MstFloorplanUpdateDto dto)
        {
            var floorplan = await _context.MstFloorplans.FindAsync(Id);
            if (floorplan == null || floorplan.Status == 0)
                throw new KeyNotFoundException("Floorplan not found");

            // Validasi FloorId jika berubah
            if (floorplan.FloorId != dto.FloorId)
            {
                var floor = await _context.MstFloors.FirstOrDefaultAsync(f => f.Id == dto.FloorId);
                if (floor == null)
                    throw new ArgumentException($"Floor with ID {dto.FloorId} not found.");
                floorplan.FloorId = dto.FloorId;
            }

            // Validasi ApplicationId jika berubah
            if (floorplan.ApplicationId != dto.ApplicationId)
            {
                var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");
                floorplan.ApplicationId = dto.ApplicationId;
            }

            floorplan.UpdatedBy ??= "";
            floorplan.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, floorplan);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var floorplan = await _context.MstFloorplans.FindAsync(id);
            if (floorplan == null || floorplan.Status == 0)
                throw new KeyNotFoundException("Floorplan not found");

            floorplan.Status = 0; // Soft delete
            await _context.SaveChangesAsync();
        }
    }
}