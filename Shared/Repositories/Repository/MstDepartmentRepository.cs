using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class MstDepartmentRepository : BaseRepository
    {

        public MstDepartmentRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<(List<MstDepartmentRead> Data, int Total, int Filtered)> FilterAsync(MstDepartmentFilter filter)
        {
            var query = GetAllQueryable();

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                string searchLower = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(searchLower) || x.Code.ToLower().Contains(searchLower));
            }

            int total = await query.CountAsync();
            int filtered = total;

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<MstDepartment> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(d => d.Id == id && d.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Department not found");
        }

        public async Task<IEnumerable<MstDepartmentRead>> GetAllAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
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

        public IQueryable<MstDepartmentRead> ProjectToRead(IQueryable<MstDepartment> query)
        {
            return query.AsNoTracking().Select(x => new MstDepartmentRead
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DepartmentHost = x.DepartmentHost,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                UpdatedAt = x.UpdatedAt,
                UpdatedBy = x.UpdatedBy,
                Status = x.Status,
                ApplicationId = x.ApplicationId
            });
        }

        public IQueryable<MstDepartment> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstDepartments
                .Where(d => d.Status != 0);
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<MstDepartmentRead>> GetAllExportAsync()
        {
            return await ProjectToRead(GetAllQueryable()).ToListAsync();
        }
    }
}