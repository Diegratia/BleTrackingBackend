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
    public class MstAccessCctvService : IMstAccessCctvService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstAccessCctvService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstAccessCctvDto> GetByIdAsync(Guid id)
        {
            var accessCctv = await _context.MstAccessCctvs
                .FirstOrDefaultAsync(a => a.Id == id);
            return accessCctv == null ? null : _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task<IEnumerable<MstAccessCctvDto>> GetAllAsync()
        {
            var accessCctvs = await _context.MstAccessCctvs.ToListAsync();
            return _mapper.Map<IEnumerable<MstAccessCctvDto>>(accessCctvs);
        }

        public async Task<MstAccessCctvDto> CreateAsync(MstAccessCctvCreateDto createDto)
        {
            var accessCctv = _mapper.Map<MstAccessCctv>(createDto);

            accessCctv.Status = 1;
             accessCctv.CreatedBy = "";
             accessCctv.UpdatedBy = "";

            _context.MstAccessCctvs.Add(accessCctv);

            // notes untuk nanti, jika ingin memisahkan service dan repository, bisa memisahkan proses logika bisnis
            // dengan service, dan proses database dengan repository, contohnya pada _context
            await _context.SaveChangesAsync();
            return _mapper.Map<MstAccessCctvDto>(accessCctv);
        }

        public async Task UpdateAsync(Guid id, MstAccessCctvUpdateDto updateDto)
        {
            var accessCctv = await _context.MstAccessCctvs.FindAsync(id);
            if (accessCctv == null)
                throw new KeyNotFoundException("Access CCTV not found");

            accessCctv.UpdatedBy = "";
            
            _mapper.Map(updateDto, accessCctv);
            // _context.MstAccessCctvs.Update(accessCctv);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var accessCctv = await _context.MstAccessCctvs.FindAsync(id);
            if (accessCctv == null)
                throw new KeyNotFoundException("Access CCTV not found");
            
            accessCctv.Status = 0;

            // _context.MstAccessCctvs.Remove(accessCctv);
            await _context.SaveChangesAsync();
        }
    }
}