using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class BoundaryRepository : BaseRepository
    {
        public BoundaryRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<Boundary> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id && a.Status != 0)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Boundary>> GetAllAsync()
        {
             return await GetAllQueryable().ToListAsync();
        }

        public async Task<Boundary> AddAsync(Boundary boundary)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                boundary.ApplicationId = applicationId.Value;
            }
            else if (boundary.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(boundary.ApplicationId);
            ValidateApplicationIdForEntity(boundary, applicationId, isSystemAdmin);

            _context.Boundarys.Add(boundary);
            await _context.SaveChangesAsync();
            return boundary;
        }

        public async Task UpdateAsync(Boundary boundary)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(boundary.ApplicationId);
            ValidateApplicationIdForEntity(boundary, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Boundarys
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var boundary = await query.FirstOrDefaultAsync();

            if (boundary == null)
                return;

            await _context.SaveChangesAsync();
        }

        public IQueryable<Boundary> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Boundarys
            .Include(d => d.Floor)
            .Include(d => d.Floorplan)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
    }
}
