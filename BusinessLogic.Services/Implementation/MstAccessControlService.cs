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
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstAccessControlService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstAccessControlDto> GetByIdAsync(Guid id)
        {
            var accessControl = await _context.MstAccessControls
                .FirstOrDefaultAsync(a => a.Id == id);
            return accessControl == null ? null : _mapper.Map<MstAccessControlDto>(accessControl);
        }

        public async Task<IEnumerable<MstAccessControlDto>> GetAllAsync()
        {
            var accessControls = await _context.MstAccessControls.ToListAsync();
            return _mapper.Map<IEnumerable<MstAccessControlDto>>(accessControls);
        }

        public async Task<MstAccessControlDto> CreateAsync(MstAccessControlCreateDto createDto)
        {
            var accessControl = _mapper.Map<MstAccessControl>(createDto);
            
             accessControl.Status = 1;
             accessControl.CreatedBy = "";
             accessControl.UpdatedBy = "";

            _context.MstAccessControls.Add(accessControl);
            await _context.SaveChangesAsync();
            return _mapper.Map<MstAccessControlDto>(accessControl);
        }

        public async Task UpdateAsync(Guid id, MstAccessControlUpdateDto updateDto)
        {
            var accessControl = await _context.MstAccessControls.FindAsync(id);
            if (accessControl == null)
                throw new KeyNotFoundException("Access Control not found");

            accessControl.UpdatedBy = "";

            _mapper.Map(updateDto, accessControl);
            // _context.MstAccessControls.Update(accessControl);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var accessControl = await _context.MstAccessControls.FindAsync(id);
            if (accessControl == null)
                throw new KeyNotFoundException("Access Control not found");

            accessControl.Status = 0;
            // _context.MstAccessControls.Remove(accessControl);
            await _context.SaveChangesAsync();
        }
    }
}