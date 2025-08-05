using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class MstMemberRepository : BaseRepository
    {
        public MstMemberRepository(BleTrackingDbDevContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<List<MstMember>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<MstMember?> GetByIdAsync(Guid id)
        {
            
            return await GetAllQueryable()
            .Where(x => x.Id == id && x.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Member not found");
        }

        public async Task AddAsync(MstMember member)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");

                member.ApplicationId = applicationId.Value;
            }
            else if (member.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(member.ApplicationId);
            ValidateApplicationIdForEntity(member, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(member, applicationId, isSystemAdmin);

            await _context.MstMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstMember member)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(member.ApplicationId);
            ValidateApplicationIdForEntity(member, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(member, applicationId, isSystemAdmin);

            // _context.MstMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstMembers
                .Where(d => d.Id == id && d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            var member = await query.FirstOrDefaultAsync();

            if (!isSystemAdmin && member.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("Cannot delete member from a different application.");

            // _context.MstMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstMember> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstMembers
                .Include(x => x.Department)
                .Include(x => x.District)
                .Include(x => x.Organization)
                .Where(x => x.Status != 0);

                query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

       public async Task<IEnumerable<MstMember>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        private async Task ValidateRelatedEntitiesAsync(MstMember member, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin user.");

            // Validate Department
            var dept = await _context.MstDepartments
                .WithActiveRelations()
                .FirstOrDefaultAsync(d => d.Id == member.DepartmentId && d.ApplicationId == applicationId);
            if (dept == null)
                throw new UnauthorizedAccessException("Invalid DepartmentId for this application.");

            // Validate District
            var district = await _context.MstDistricts
                .WithActiveRelations()
                .FirstOrDefaultAsync(d => d.Id == member.DistrictId && d.ApplicationId == applicationId);
            if (district == null)
                throw new UnauthorizedAccessException("Invalid DistrictId for this application.");

            // Validate Organization
            var org = await _context.MstOrganizations
             .WithActiveRelations()
                .FirstOrDefaultAsync(o => o.Id == member.OrganizationId && o.ApplicationId == applicationId);
            if (org == null)
                throw new UnauthorizedAccessException("Invalid OrganizationId for this application.");
        }
    }
}
