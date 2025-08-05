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
    public class MstOrganizationRepository : BaseRepository
    {
        public MstOrganizationRepository(BleTrackingDbDevContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<MstOrganization>> GetAllAsync()
        {
            return await GetAllQueryable()
            .ToListAsync();
        }

        public async Task<MstOrganization> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(o => o.Id == id && o.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Organization not found");
        }

        public async Task<MstOrganization> AddAsync(MstOrganization organization)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                organization.ApplicationId = applicationId.Value;
            }
            else if (organization.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(organization.ApplicationId);
            ValidateApplicationIdForEntity(organization, applicationId, isSystemAdmin);

            _context.MstOrganizations.Add(organization);
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task UpdateAsync(MstOrganization organization)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(organization.ApplicationId);
            ValidateApplicationIdForEntity(organization, applicationId, isSystemAdmin);

            // _context.MstOrganizations.Update(organization); // Optional
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstOrganizations
                .Where(o => o.Id == id && o.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var organization = await query.FirstOrDefaultAsync();

            if (organization == null)
                throw new KeyNotFoundException("Organization not found");

            organization.Status = 0;
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstOrganization> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstOrganizations
                .Where(o => o.Status != 0);

            query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }


        public async Task<IEnumerable<MstOrganization>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
    }
}
