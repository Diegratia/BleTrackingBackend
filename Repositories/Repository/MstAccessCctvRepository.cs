using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class MstAccessCctvRepository : BaseRepository
    {
        public MstAccessCctvRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<MstAccessCctv?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(a => a.Id == id && a.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MstAccessCctv>> GetAllAsync()
        {
            
            return await GetAllQueryable().ToListAsync();
        }
        
        public async Task<IEnumerable<MstAccessCctv>> GetAllUnassignedAsync()
        {
            return await GetAllUnassignedQueryable().ToListAsync();
        }

    public async Task<MstAccessCctv> AddAsync(MstAccessCctv accessCctv)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");
                accessCctv.ApplicationId = applicationId.Value;
            }
            else if (accessCctv.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(accessCctv.ApplicationId);
            ValidateApplicationIdForEntity(accessCctv, applicationId, isSystemAdmin);

            // var integration = await _context.MstIntegrations
            // .FirstOrDefaultAsync(i => i.Id == accessCctv.IntegrationId && i.Status != 0);

            //     if (integration == null)
            //         throw new KeyNotFoundException("Referenced integration not found.");

            //     if (!isSystemAdmin && integration.ApplicationId != applicationId)
            //         throw new UnauthorizedAccessException("Integration does not belong to the same Application.");

            _context.MstAccessCctvs.Add(accessCctv);
            await _context.SaveChangesAsync();

            return accessCctv;
        }

        public async Task UpdateAsync(MstAccessCctv accessCctv)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(accessCctv.ApplicationId);
            ValidateApplicationIdForEntity(accessCctv, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessCctvs
                .Where(a => a.Id == id && a.Status != 0);

            var accessCctv = await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();

            await _context.SaveChangesAsync();
        }

        public IQueryable<MstAccessCctv> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessCctvs
                .Include(a => a.Integration)
                .Where(a => a.Status != 0);

            query = query.WithActiveRelations();    

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

          public IQueryable<MstAccessCctv> GetAllUnassignedQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstAccessCctvs
                .Include(r => r.Integration)
                .Where(r => r.IsAssigned == false && r.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

         public async Task<IEnumerable<MstAccessCctv>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
    }
}
