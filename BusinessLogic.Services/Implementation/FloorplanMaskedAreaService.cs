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
    public class FloorplanMaskedAreaService : IFloorplanMaskedAreaService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public FloorplanMaskedAreaService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<FloorplanMaskedAreaDto> GetByIdAsync(Guid id)
        {
            var area = await _context.FloorplanMaskedAreas
                .Include (f => f.Floor)
                .FirstOrDefaultAsync(a => a.Id == id);
            return area == null ? null : _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task<IEnumerable<FloorplanMaskedAreaDto>> GetAllAsync()
        {
            var areas = await _context.FloorplanMaskedAreas
            .Include(f => f.Floor)
            .ToListAsync();
            return _mapper.Map<IEnumerable<FloorplanMaskedAreaDto>>(areas);
        }

        public async Task<FloorplanMaskedAreaDto> CreateAsync(FloorplanMaskedAreaCreateDto createDto)
        {
            var floor = await _context.MstFloors.FirstOrDefaultAsync(a => a.Id == createDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {createDto.FloorId} not found.");

            var area = _mapper.Map<FloorplanMaskedArea>(createDto);

             area.Status = 1;
             area.CreatedBy = "";
             area.UpdatedBy = "";

            _context.FloorplanMaskedAreas.Add(area);
            await _context.SaveChangesAsync();
             var savedArea = await _context.FloorplanMaskedAreas
                .Include(d => d.Floor)
                .FirstOrDefaultAsync(d => d.Id == area.Id);
            return _mapper.Map<FloorplanMaskedAreaDto>(area);
        }

        public async Task UpdateAsync(Guid id, FloorplanMaskedAreaUpdateDto updateDto)
        {
            var floor = await _context.MstFloors.FirstOrDefaultAsync(a => a.Id == updateDto.FloorId);
            if (floor == null)
                throw new ArgumentException($"Floor with ID {updateDto.FloorId} not found.");

            var area = _mapper.Map<FloorplanMaskedArea>(updateDto);

            area.UpdatedBy = "";

            _mapper.Map(updateDto, area);
            _context.FloorplanMaskedAreas.Update(area);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var area = await _context.FloorplanMaskedAreas.FindAsync(id);
            if (area == null)
                throw new KeyNotFoundException("Area not found");

            area.Status = 0;

            // _context.FloorplanMaskedAreas.Remove(area);
            await _context.SaveChangesAsync();
        }
    }
}