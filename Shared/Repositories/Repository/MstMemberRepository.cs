using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using Repositories.Extensions;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Repositories.Repository
{
    public class MstMemberRepository : BaseRepository
    {
        public MstMemberRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        #region Base Query & Projection

        /// <summary>
        /// Base query with multi-tenancy filtering and active status check
        /// </summary>
        private IQueryable<MstMember> BaseEntityQuery()
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

        /// <summary>
        /// Manual projection to MstMemberRead - single source of truth for read operations
        /// </summary>
        private IQueryable<MstMemberRead> ProjectToRead(IQueryable<MstMember> query)
        {
            return query
                .Select(m => new MstMemberRead
                {
                    Id = m.Id,
                    Generate = m.Generate,
                    PersonId = m.PersonId,
                    OrganizationId = m.OrganizationId,
                    DepartmentId = m.DepartmentId,
                    DistrictId = m.DistrictId,
                    IdentityId = m.IdentityId,
                    CardNumber = m.CardNumber,
                    BleCardNumber = m.BleCardNumber,
                    Name = m.Name,
                    Phone = m.Phone,
                    Email = m.Email,
                    Gender = m.Gender.ToString(),
                    Address = m.Address,
                    FaceImage = m.FaceImage,
                    UploadFr = m.UploadFr,
                    UploadFrError = m.UploadFrError,
                    BirthDate = m.BirthDate,
                    JoinDate = m.JoinDate,
                    ExitDate = m.ExitDate,
                    HeadMember1 = m.HeadMember1,
                    HeadMember2 = m.HeadMember2,
                    IsBlacklist = m.IsBlacklist,
                    BlacklistAt = m.BlacklistAt,
                    BlacklistReason = m.BlacklistReason,
                    StatusEmployee = m.StatusEmployee.ToString(),
                    Status = m.Status,
                    ApplicationId = m.ApplicationId,
                    CreatedBy = m.CreatedBy,
                    CreatedAt = m.CreatedAt,
                    UpdatedBy = m.UpdatedBy,
                    UpdatedAt = m.UpdatedAt,
                    Organization = m.Organization != null ? new OrganizationRead
                    {
                        Id = m.Organization.Id,
                        Name = m.Organization.Name
                    } : null,
                    Department = m.Department != null ? new DepartmentRead
                    {
                        Id = m.Department.Id,
                        Name = m.Department.Name
                    } : null,
                    District = m.District != null ? new DistrictRead
                    {
                        Id = m.District.Id,
                        Name = m.District.Name
                    } : null
                });
        }

        #endregion

        #region Read Operations (Return Read DTOs)

        /// <summary>
        /// Get member by ID - returns Read DTO
        /// </summary>
        public async Task<MstMemberRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Get all members - returns Read DTOs
        /// </summary>
        public async Task<IEnumerable<MstMemberRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        /// <summary>
        /// Get member as entity (for update/delete operations)
        /// </summary>
        public async Task<MstMember?> GetByIdEntityAsync(Guid id)
        {
            return await GetAllQueryable()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Filter members with pagination and sorting
        /// </summary>
        public async Task<(List<MstMemberRead> Data, int Total, int Filtered)> FilterAsync(MemberFilter filter)
        {
            var query = BaseEntityQuery();

            // Search filter
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(s)
                    || (x.PersonId != null && x.PersonId.ToLower().Contains(s))
                    || (x.Email != null && x.Email.ToLower().Contains(s))
                    || (x.CardNumber != null && x.CardNumber.ToLower().Contains(s))
                    || (x.IdentityId != null && x.IdentityId.ToLower().Contains(s)));
            }

            // Specific filters
            if (filter.IsBlacklist.HasValue)
                query = query.Where(x => x.IsBlacklist == filter.IsBlacklist.Value);

            if (filter.OrganizationId.HasValue)
                query = query.Where(x => x.OrganizationId == filter.OrganizationId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == filter.DepartmentId.Value);

            if (filter.DistrictId.HasValue)
                query = query.Where(x => x.DistrictId == filter.DistrictId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(x => x.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(x => x.Name.ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
                query = query.Where(x => x.CardNumber.ToLower().Contains(filter.CardNumber.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.PersonId))
                query = query.Where(x => x.PersonId.ToLower().Contains(filter.PersonId.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.StatusEmployee))
                query = query.Where(x => x.StatusEmployee.ToString() == filter.StatusEmployee);

            // Date range filter
            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            // Use extension methods for sorting and paging
            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            // Use ProjectToRead for single source of truth
            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        #endregion

        #region Ownership Validation

        /// <summary>
        /// Validate Card ownership for Member-Card relationship
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidCardOwnershipAsync(
            Guid cardId, Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<Card>(
                new[] { cardId },
                applicationId);
        }

        #endregion

        #region Legacy Methods (Preserved for backward compatibility)

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

        // Legacy: Returns entities for export operations
        // Use GetAllAsync() (returns MstMemberRead) for normal read operations
        public async Task<List<MstMember>> GetAllEntitiesAsync()
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

          public async Task<List<MstMemberLookUpRead>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstMembers
            .AsNoTracking()
            .Where(fd => fd.Status != 0 && fd.CardNumber != null);

            var projected = query.Select(t => new MstMemberLookUpRead
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
                Email = t.Email
            });
            return await projected.ToListAsync();
        }
        


        // Renamed old GetByIdAsync to GetByIdLegacy for backward compatibility if needed
        // Use GetByIdAsync (returns MstMemberRead) for read operations
        // Use GetByIdEntityAsync (returns MstMember) for update/delete operations

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
                .Where(u => u.Id == id && u.Status != 0);
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
            return await GetAllEntitiesAsync();
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
        #endregion
    }
}