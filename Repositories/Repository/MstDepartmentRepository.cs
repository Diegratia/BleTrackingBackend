    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities.Models;
    using Microsoft.EntityFrameworkCore;
    using Repositories.DbContexts;
    using Microsoft.AspNetCore.Http;
    using System.Security.Claims;

    namespace Repositories.Repository
    {
        public class MstDepartmentRepository : BaseRepository
        {

            public MstDepartmentRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
                : base(context, httpContextAccessor)
            {
            }

            public async Task<MstDepartment> GetByIdAsync(Guid id)
            {   
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                var query = _context.MstDepartments
                    .Where(d => d.Id == id && d.Status != 0);
                query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
                return await _context.MstDepartments
                    .FirstOrDefaultAsync();
            }
            


          public async Task<IEnumerable<MstDepartment>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstDepartments
                .Where(d => d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.ToListAsync();
        }

            public async Task<MstDepartment> AddAsync(MstDepartment department)
            {
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                 // non system ambil dari claim
                if (!isSystemAdmin)
                {
                    if (!applicationId.HasValue)
                        throw new UnauthorizedAccessException("ApplicationId not found in context");
                    department.ApplicationId = applicationId.Value;
                }
                // admin set applciation di body
                else if (department.ApplicationId == Guid.Empty)
                {
                    throw new ArgumentException("System admin must provide a valid ApplicationId");
                }
                await ValidateApplicationIdAsync(department.ApplicationId);
                ValidateApplicationIdForEntity(department, applicationId, isSystemAdmin);
             
                _context.MstDepartments.Add(department);
                await _context.SaveChangesAsync();
                return department;
            }

            public async Task UpdateAsync(MstDepartment department)
            {
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                await ValidateApplicationIdAsync(department.ApplicationId);
            
                ValidateApplicationIdForEntity(department, applicationId, isSystemAdmin);

                // _context.MstDepartments.Update(department);
                await _context.SaveChangesAsync();
            }

            public async Task DeleteAsync(Guid id)
            {
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                var query = _context.MstDepartments
                    .Where(d => d.Id == id && d.Status != 0);
                query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

                var department = await query.FirstOrDefaultAsync();
                if (department == null)
                    throw new KeyNotFoundException("Department not found");

                department.Status = 0;
                await _context.SaveChangesAsync();
            }

            public IQueryable<MstDepartment> GetAllQueryable()
            {
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                var query = _context.MstDepartments
                    .Where(d => d.Status != 0);
                return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            }
            
            public async Task<IEnumerable<MstDepartment>> GetAllExportAsync()
            {
                var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
                var query = _context.MstDepartments
                    .Where(d => d.Status != 0);
                query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
                return await query.ToListAsync();
            }
        }
    }