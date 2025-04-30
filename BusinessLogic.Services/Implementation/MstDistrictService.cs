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
    public class MstDistrictService : IMstDistrictService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstDistrictService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstDistrictDto> GetByIdAsync(Guid id)
        {
            var district = await _context.MstDistricts
                // .Include (d => d.Application)
                .FirstOrDefaultAsync(d => d.Id == id);
            return district == null ? null : _mapper.Map<MstDistrictDto>(district);
        }

        public async Task<IEnumerable<MstDistrictDto>> GetAllAsync()
        {
            var districts = await _context.MstDistricts
            // .Include (d => d.Application) 
            .ToListAsync();
            return _mapper.Map<IEnumerable<MstDistrictDto>>(districts);
        }

        public async Task<MstDistrictDto> CreateAsync(MstDistrictCreateDto createDto)
        {
             // validasi untuk application
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            //nanti ganti dengan logic data login user
            var district = _mapper.Map<MstDistrict>(createDto);
            district.CreatedBy = ""; 
            district.UpdatedBy = ""; 

            _context.MstDistricts.Add(district);
            await _context.SaveChangesAsync();

            //return distirct dengan application
             var savedDistrict = await _context.MstDistricts
                .Include(i => i.Application)
                .FirstOrDefaultAsync(i => i.Id == district.Id);

            return _mapper.Map<MstDistrictDto>(district);
        }

        public async Task UpdateAsync(Guid id, MstDistrictUpdateDto updateDto)
        {       
            var district = await _context.MstDistricts.FindAsync(id);
            if (district == null)
                throw new KeyNotFoundException("District not found");


             // validasi Application
            if (district.ApplicationId != updateDto.ApplicationId)
            {
                var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == updateDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
                district.ApplicationId = updateDto.ApplicationId;
            }

            district.UpdatedBy ??= "";
            _mapper.Map(updateDto, district);
            // _context.MstDistricts.Update(district);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var district = await _context.MstDistricts.FindAsync(id);
            if (district == null)
                throw new KeyNotFoundException("District not found");
            
            district.Status = 0;

            // _context.MstDistricts.Remove(district);
            await _context.SaveChangesAsync();
        }
    }
}