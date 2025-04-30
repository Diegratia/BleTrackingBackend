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
    public class MstBuildingService : IMstBuildingService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstBuildingService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstBuildingDto> GetByIdAsync(Guid id)
        {
            var building = await _context.MstBuildings
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);

            return building == null ? null : _mapper.Map<MstBuildingDto>(building);
        }

        public async Task<IEnumerable<MstBuildingDto>> GetAllAsync()
        {
            var buildings = await _context.MstBuildings
                .Where(b => b.Status != 0)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MstBuildingDto>>(buildings);
        }

        public async Task<MstBuildingDto> CreateAsync(MstBuildingCreateDto dto)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");

            var building = _mapper.Map<MstBuilding>(dto);
            building.Id = Guid.NewGuid();
            building.Status = 1;
            building.CreatedAt = DateTime.UtcNow;
            building.UpdatedAt = DateTime.UtcNow;
            building.CreatedBy ??= "";
            building.UpdatedBy ??= "";

            _context.MstBuildings.Add(building);
            await _context.SaveChangesAsync();

            // Kembalikan MstBuildingDto dengan data dasar
            return _mapper.Map<MstBuildingDto>(building);
        }

        public async Task UpdateAsync(Guid Id, MstBuildingUpdateDto dto)
        {
            var building = await _context.MstBuildings.FindAsync(Id);
            if (building == null || building.Status == 0)
                throw new KeyNotFoundException("Building not found");

            // Validasi ApplicationId jika berubah
            if (building.ApplicationId != dto.ApplicationId)
            {
                var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == dto.ApplicationId);
                if (application == null)
                    throw new ArgumentException($"Application with ID {dto.ApplicationId} not found.");
                building.ApplicationId = dto.ApplicationId;
            }

            building.UpdatedBy ??= "";
            building.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, building);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var building = await _context.MstBuildings.FindAsync(id);
            if (building == null || building.Status == 0)
                throw new KeyNotFoundException("Building not found");

            building.Status = 0; // Soft delete
            await _context.SaveChangesAsync();
        }
    }
}