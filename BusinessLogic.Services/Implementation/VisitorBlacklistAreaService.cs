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
    public class VisitorBlacklistAreaService : IVisitorBlacklistAreaService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public VisitorBlacklistAreaService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<VisitorBlacklistAreaDto> CreateVisitorBlacklistAreaAsync(VisitorBlacklistAreaCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            if (!await _context.FloorplanMaskedAreas.AnyAsync(f => f.Id == dto.FloorplanMaskedAreaId))
                throw new ArgumentException($"FloorplanMaskedArea with ID {dto.FloorplanMaskedAreaId} not found.");
            if (!await _context.Visitors.AnyAsync(v => v.Id == dto.VisitorId))
                throw new ArgumentException($"Visitor with ID {dto.VisitorId} not found.");

            var blacklistArea = _mapper.Map<VisitorBlacklistArea>(dto);
            blacklistArea.Id = Guid.NewGuid();

            _context.VisitorBlacklistAreas.Add(blacklistArea);
            await _context.SaveChangesAsync();

            return _mapper.Map<VisitorBlacklistAreaDto>(blacklistArea);
        }

        public async Task<VisitorBlacklistAreaDto> GetVisitorBlacklistAreaByIdAsync(Guid id)
        {
            var blacklistArea = await _context.VisitorBlacklistAreas
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .FirstOrDefaultAsync(v => v.Id == id);
            return _mapper.Map<VisitorBlacklistAreaDto>(blacklistArea);
        }

        public async Task<IEnumerable<VisitorBlacklistAreaDto>> GetAllVisitorBlacklistAreasAsync()
        {
            var blacklistAreas = await _context.VisitorBlacklistAreas
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .ToListAsync();
            return _mapper.Map<IEnumerable<VisitorBlacklistAreaDto>>(blacklistAreas);
        }

        public async Task UpdateVisitorBlacklistAreaAsync(Guid id, VisitorBlacklistAreaUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var blacklistArea = await _context.VisitorBlacklistAreas.FindAsync(id);
            if (blacklistArea == null)
            {
                throw new KeyNotFoundException($"VisitorBlacklistArea with ID {id} not found.");
            }
                //validasi FloorplanMaskedArea
                 if (blacklistArea.FloorplanMaskedAreaId != updateDto.FloorplanMaskedAreaId)
            {
                var floorplanmaskedarea = await _context.FloorplanMaskedAreas.FirstOrDefaultAsync(a => a.Id == updateDto.FloorplanMaskedAreaId);
                    if (!await _context.FloorplanMaskedAreas.AnyAsync(f => f.Id == updateDto.FloorplanMaskedAreaId))
                    throw new ArgumentException($"FloorplanMaskedArea with ID {updateDto.FloorplanMaskedAreaId} not found.");
                blacklistArea.FloorplanMaskedAreaId = updateDto.FloorplanMaskedAreaId;
            }
                  // validasi visitor
                if (blacklistArea.VisitorId != updateDto.VisitorId)
            {
                var visitor = await _context.Visitors.FirstOrDefaultAsync(a => a.Id == updateDto.VisitorId);
                if (!await _context.Visitors.AnyAsync(v => v.Id == updateDto.VisitorId))
                throw new ArgumentException($"Visitor with ID {updateDto.VisitorId} not found.");
                blacklistArea.VisitorId = updateDto.VisitorId;
            }

            _mapper.Map(updateDto, blacklistArea);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVisitorBlacklistAreaAsync(Guid id)
        {
            var blacklistArea = await _context.VisitorBlacklistAreas.FindAsync(id);
            if (blacklistArea == null)
            {
                throw new KeyNotFoundException($"VisitorBlacklistArea with ID {id} not found.");
            }

            _context.VisitorBlacklistAreas.Remove(blacklistArea);
            await _context.SaveChangesAsync();
        }
    }
}