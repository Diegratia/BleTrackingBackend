using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstDepartmentRepository
    {
        private readonly BleTrackingDbContext _context;

        public MstDepartmentRepository(BleTrackingDbContext context)
        {
            _context = context;
        }

        public async Task<MstDepartment> GetByIdAsync(Guid id)
        {
            return await _context.MstDepartments
                .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
        }

        public async Task<IEnumerable<MstDepartment>> GetAllAsync()
        {
            return await _context.MstDepartments
                .Where(d => d.Status != 0)
                .ToListAsync();
        }

        public async Task<MstDepartment> AddAsync(MstDepartment department)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == department.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {department.ApplicationId} not found.");

            _context.MstDepartments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task UpdateAsync(MstDepartment department)
        {
            // Validasi ApplicationId
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == department.ApplicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {department.ApplicationId} not found.");

            // _context.MstDepartments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var department = await _context.MstDepartments
                .FirstOrDefaultAsync(d => d.Id == id && d.Status != 0);
            if (department == null)
                throw new KeyNotFoundException("Department not found");

            department.Status = 0;
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstDepartment> GetAllQueryable()
        {
            return _context.MstDepartments
                .Where(f => f.Status != 0)
                .AsQueryable();
        }
        
          public async Task<IEnumerable<MstDepartment>> GetAllExportAsync()
        {
            return await _context.MstDepartments
                .Where(d => d.Status != 0)
                .ToListAsync();
        }
    }
}