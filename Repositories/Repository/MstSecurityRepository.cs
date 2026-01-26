using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class MstSecurityRepository : BaseRepository
    {
        public MstSecurityRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

            public async Task<MstOrganization?> GetOrganizationByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstOrganizations
                .Where(b => b.Id == id && b.Status != 0);
                query = query.WithActiveRelations();
            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }
            public async Task<MstDistrict?> GetDistrictByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstDistricts
                .Where(b => b.Id == id && b.Status != 0);
                // query = query.WithActiveRelations();
            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }
            public async Task<MstDepartment?> GetDepartmentByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstDepartments
                .Where(b => b.Id == id && b.Status != 0);
                query = query.WithActiveRelations();
            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<List<MstSecurity>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        // public async Task<List<MstSecurityBlacklistLogRM>> GetBlacklistedAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.MstSecurities
        //                 .AsNoTracking()
        //                 .Where(fd => fd.Status != 0 && fd.IsBlacklist == true);
        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

        //     var projected = query.Select(t => new MstSecurityBlacklistLogRM
        //         {
        //             Id = t.Id,
        //             Name = t.Name,
        //             CardNumber = t.CardNumber,
        //             PersonId = t.PersonId,
        //         });
        //         return await projected.ToListAsync();                 
        // }

        //     public async Task<List<BlacklistLogRM>> GetBlacklistLogsAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     // === Query MEMBER ===
        //     var memberQuery = _context.MstSecurities
        //         .AsNoTracking()
        //         .Where(x => x.Status != 0 && x.IsBlacklist == true);

        //     memberQuery = ApplyApplicationIdFilter(memberQuery, applicationId, isSystemAdmin);

        //     var memberLogs = await memberQuery
        //         .Select(t => new BlacklistLogRM
        //         {
        //             Id = t.Id,
        //             Type = "Member",
        //             Name = t.Name,
        //             CardNumber = t.CardNumber,
        //             PersonId = t.PersonId,
        //         })
        //         .ToListAsync();


        //     // === Query VISITOR ===
        //     var visitorQuery = _context.Visitors
        //         .AsNoTracking()
        //         .Where(x => x.Status != 0 && x.IsBlacklist == true);

        //     visitorQuery = ApplyApplicationIdFilter(visitorQuery, applicationId, isSystemAdmin);

        //     var visitorLogs = await visitorQuery
        //         .Select(t => new BlacklistLogRM
        //         {
        //             Id = t.Id,
        //             Type = "Visitor",
        //             Name = t.Name,
        //             CardNumber = t.CardNumber,
        //             PersonId = t.PersonId,
        //         })
        //         .ToListAsync();

        //     // === Kombinasikan ===
        //     return memberLogs
        //         .Concat(visitorLogs)
        //         .OrderByDescending(x => x.Id)   // optional: sorting gabungan
        //         .ToList();
        // }

        

        // public async Task<int> GetBlacklistedCountAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var q = _context.MstSecurities
        //         .AsNoTracking()
        //         .Where(c => c.Status != 0 && c.IsBlacklist == true);

        //     q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

        //     return await q.CountAsync();
        // }

          public async Task<List<MstSecurityLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstSecurities
            .AsNoTracking()
            .Where(fd => fd.Status != 0 && fd.CardNumber != null);

            var projected = query.Select(t => new MstSecurityLookUpRM
            {
                Id = t.Id,
                Name = t.Name,
                PersonId = t.PersonId,
                CardNumber = t.CardNumber,
                OrganizationId = t.OrganizationId,
                DepartmentId = t.DepartmentId,
                DistrictId = t.DistrictId,
                OrganizationName = t.Organization.Name,
                DepartmentName = t.Department.Name,
                DistrictName = t.District.Name,
                ApplicationId = t.ApplicationId
            }); 
            return await projected.ToListAsync();
        }
        


        public async Task<MstSecurity?> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(x => x.Id == id && x.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<MstSecurity?> GetByIdRawAsync(Guid id)
        {
            var query = _context.MstSecurities
                .Include(x => x.Department)
                .Include(x => x.District)
                .Include(x => x.Organization)
            .Where(x => x.Id == id && x.Status != 0);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<User> GetByIdAsyncRaw(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.StatusActive != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }

        public async Task AddAsync(MstSecurity member)
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

            await _context.MstSecurities.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstSecurity member)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(member.ApplicationId);
            ValidateApplicationIdForEntity(member, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(member, applicationId, isSystemAdmin);

            // _context.MstSecurities.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstSecurities
                .Where(d => d.Id == id && d.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            var member = await query.FirstOrDefaultAsync();

            if (!isSystemAdmin && member.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("Cannot delete Security from a different application.");

            // _context.MstSecurities.Update(member);
            await _context.SaveChangesAsync();
        }

        public IQueryable<MstSecurity> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstSecurities
                .Include(x => x.Department)
                .Include(x => x.District)
                .Include(x => x.Organization)
                .Where(x => x.Status != 0);

            query = query.WithActiveRelations();
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

       public async Task<IEnumerable<MstSecurity>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

                public async Task<MstSecurity?> GetByEmailAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstSecurities
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<MstSecurity?> GetByEmailAsyncRaw(string email)
        {
            var query = _context.MstSecurities
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            return await query.FirstOrDefaultAsync();
        }

        // public async Task<MstSecurity> GetByEmailAsyncRaw(string email)
        // {
        //     return await _context.MstSecurities
        //         .FirstOrDefaultAsync(m => m.Email == email);
        // }

        private async Task ValidateRelatedEntitiesAsync(MstSecurity security, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin user.");

            // Validate Department
            var dept = await _context.MstDepartments
                .WithActiveRelations()
                .FirstOrDefaultAsync(d => d.Id == security.DepartmentId && d.ApplicationId == applicationId);
            if (dept == null)
                throw new UnauthorizedAccessException("Invalid DepartmentId for this application.");

            // Validate District
            var district = await _context.MstDistricts
                .WithActiveRelations()
                .FirstOrDefaultAsync(d => d.Id == security.DistrictId && d.ApplicationId == applicationId);
            if (district == null)
                throw new UnauthorizedAccessException("Invalid DistrictId for this application.");

            // Validate Organization
            var org = await _context.MstOrganizations
             .WithActiveRelations()
                .FirstOrDefaultAsync(o => o.Id == security.OrganizationId && o.ApplicationId == applicationId);
            if (org == null)
                throw new UnauthorizedAccessException("Invalid OrganizationId for this application.");
        }
    }
}
