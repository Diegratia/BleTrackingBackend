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
    public class MstMemberRepository : BaseRepository
    {
        public MstMemberRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
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

        public async Task<List<MstMember>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<List<MstMemberBlacklistLogRM>> GetBlacklistedAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstMembers
                        .AsNoTracking()
                        .Where(fd => fd.Status != 0 && fd.IsBlacklist == true);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var projected = query.Select(t => new MstMemberBlacklistLogRM
                {
                    Id = t.Id,
                    Name = t.Name,
                    CardNumber = t.CardNumber,
                    PersonId = t.PersonId,
                });
                return await projected.ToListAsync();                 
        }

            public async Task<List<BlacklistLogRM>> GetBlacklistLogsAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // === Query MEMBER ===
            var memberQuery = _context.MstMembers
                .AsNoTracking()
                .Where(x => x.Status != 0 && x.IsBlacklist == true);

            memberQuery = ApplyApplicationIdFilter(memberQuery, applicationId, isSystemAdmin);

            var memberLogs = await memberQuery
                .Select(t => new BlacklistLogRM
                {
                    Id = t.Id,
                    Type = "Member",
                    Name = t.Name,
                    CardNumber = t.CardNumber,
                    PersonId = t.PersonId,
                })
                .ToListAsync();


            // === Query VISITOR ===
            var visitorQuery = _context.Visitors
                .AsNoTracking()
                .Where(x => x.Status != 0 && x.IsBlacklist == true);

            visitorQuery = ApplyApplicationIdFilter(visitorQuery, applicationId, isSystemAdmin);

            var visitorLogs = await visitorQuery
                .Select(t => new BlacklistLogRM
                {
                    Id = t.Id,
                    Type = "Visitor",
                    Name = t.Name,
                    CardNumber = t.CardNumber,
                    PersonId = t.PersonId,
                })
                .ToListAsync();

            // === Kombinasikan ===
            return memberLogs
                .Concat(visitorLogs)
                .OrderByDescending(x => x.Id)   // optional: sorting gabungan
                .ToList();
        }

        

        public async Task<int> GetBlacklistedCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.MstMembers
                .AsNoTracking()
                .Where(c => c.Status != 0 && c.IsBlacklist == true);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q.CountAsync();
        }

          public async Task<List<MstMemberLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstMembers
            .AsNoTracking()
            .Where(fd => fd.Status != 0 && fd.CardNumber != null);

            var projected = query.Select(t => new MstMemberLookUpRM
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
        


        public async Task<MstMember?> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(x => x.Id == id && x.Status != 0)
            .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Member not found");
        }

        public async Task<MstMember?> GetByIdRawAsync(Guid id)
        {
            var query = _context.MstMembers
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

                public async Task<MstMember> GetByEmailAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstMembers
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Member not found");
        }

        public async Task<MstMember> GetByEmailAsyncRaw(string email)
        {
            var query = _context.MstMembers
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            return await query.FirstOrDefaultAsync();
        }

        // public async Task<MstMember> GetByEmailAsyncRaw(string email)
        // {
        //     return await _context.MstMembers
        //         .FirstOrDefaultAsync(m => m.Email == email);
        // }

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
