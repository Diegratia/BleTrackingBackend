using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using Repositories.DbContexts;

namespace BusinessLogic.Services.Implementation
{
    public class MstDepartmentService : IMstDepartmentService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public MstDepartmentService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MstDepartmentDto> GetByIdAsync(Guid id)
        {
            var department = await _context.MstDepartments
                .Include (d => d.Application)
                .FirstOrDefaultAsync(d => d.Id == id);
            return department == null ? null : _mapper.Map<MstDepartmentDto>(department);
        }

        public async Task<IEnumerable<MstDepartmentDto>> GetAllAsync()
        {
            var departments = await _context.MstDepartments
            .Include (d => d.Application)
            .ToListAsync();
            return _mapper.Map<IEnumerable<MstDepartmentDto>>(departments);
        }

        public async Task<MstDepartmentDto> CreateAsync(MstDepartmentCreateDto createDto)
        {
            // validasi untuk application
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == createDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var department = _mapper.Map<MstDepartment>(createDto);

            department.CreatedBy ??= "";
            department.UpdatedBy ??= "";
            department.Status = 1;

            _context.MstDepartments.Add(department);
            await _context.SaveChangesAsync();

            var savedDepartment = await _context.MstDepartments
                .Include(d => d.Application)
                .FirstOrDefaultAsync(d => d.Id == department.Id);
            return _mapper.Map<MstDepartmentDto>(savedDepartment);
        }

        public async Task UpdateAsync(Guid id, MstDepartmentUpdateDto updateDto)
        {
            var department = await _context.MstDepartments.FindAsync(id);
            if (department == null)
                throw new KeyNotFoundException("Department not found");

             // validasi untuk application
            var application = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == updateDto.ApplicationId);
            if (application == null)
                throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");

            department.UpdatedBy ??= "";
            _mapper.Map(updateDto, department);
            // _context.MstDepartments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var department = await _context.MstDepartments.FindAsync(id);
            if (department == null)
                throw new KeyNotFoundException("Department not found");

            department.Status = 0;

            // _context.MstDepartments.Remove(department);
            await _context.SaveChangesAsync();
        }
    }
}