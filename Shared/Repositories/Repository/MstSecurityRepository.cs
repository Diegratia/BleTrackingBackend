using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;
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

        public IQueryable<MstSecurity> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstSecurities
                .Where(x => x.Status != 0);

            query = query.WithActiveRelations();
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<MstSecurityRead> ProjectToRead(IQueryable<MstSecurity> query)
        {
            return query
                .GroupJoin(
                    _context.Users.Include(u => u.Group),
                    s => s.Email,
                    u => u.Email,
                    (s, users) => new { s, user = users.FirstOrDefault() })
                .Select(x => new MstSecurityRead
                {
                    // BaseRead properties
                    Id = x.s.Id,
                    Status = x.s.Status,
                    ApplicationId = x.s.ApplicationId,

                    // MstSecurity-specific properties
                    PersonId = x.s.PersonId,
                    OrganizationId = x.s.OrganizationId,
                    DepartmentId = x.s.DepartmentId,
                    DistrictId = x.s.DistrictId,
                    IdentityId = x.s.IdentityId,
                    CardNumber = x.s.CardNumber,
                    BleCardNumber = x.s.BleCardNumber,
                    Name = x.s.Name,
                    Phone = x.s.Phone,
                    Email = x.s.Email,
                    Gender = x.s.Gender.ToString(),
                    Address = x.s.Address,
                    FaceImage = x.s.FaceImage,
                    UploadFr = x.s.UploadFr,
                    UploadFrError = x.s.UploadFrError,
                    BirthDate = x.s.BirthDate,
                    JoinDate = x.s.JoinDate,
                    ExitDate = x.s.ExitDate,
                    HeadMember1 = x.s.SecurityHead1Id.ToString(),
                    HeadMember2 = x.s.SecurityHead2Id.ToString(),
                    IsBlacklist = null,
                    BlacklistAt = null,
                    BlacklistReason = null,
                    StatusEmployee = x.s.StatusEmployee.ToString(),
                    IsHead = x.user != null && x.user.Group != null ? x.user.Group.IsHead : null,
                    Organization = x.s.Organization != null ? new OrganizationRead
                    {
                        Id = x.s.Organization.Id,
                        Name = x.s.Organization.Name
                    } : null,
                    Department = x.s.Department != null ? new DepartmentRead
                    {
                        Id = x.s.Department.Id,
                        Name = x.s.Department.Name
                    } : null,
                    District = x.s.District != null ? new DistrictRead
                    {
                        Id = x.s.District.Id,
                        Name = x.s.District.Name
                    } : null
                });
        }

        public async Task<MstSecurityRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<MstSecurity?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<MstSecurityRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }
        // public async Task<IEnumerable<MstSecurityRead>> GetAllSecurityHeadAsync()
        // {
        //     return await ProjectToRead(BaseEntityQuery()).ToListAsync(x => x.IsHead == true);
        // }

        public async Task<(List<MstSecurityRead> Data, int Total, int Filtered)> FilterAsync(SecurityFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(x => x.Name.ToLower().Contains(filter.Search.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(x => x.Email != null && x.Email.ToLower().Contains(filter.Email.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(filter.CardNumber))
                query = query.Where(x => x.CardNumber != null && x.CardNumber.ToLower().Contains(filter.CardNumber.ToLower()));

            if (filter.OrganizationId.HasValue)
                query = query.Where(x => x.OrganizationId == filter.OrganizationId);

            if (filter.DepartmentId.HasValue)
                query = query.Where(x => x.DepartmentId == filter.DepartmentId);

            if (filter.DistrictId.HasValue)
                query = query.Where(x => x.DistrictId == filter.DistrictId);

            if (filter.IsHead.HasValue)
            {
                if (filter.IsHead.Value)
                {
                    query = query.Where(s => _context.Users
                        .Include(u => u.Group)
                        .Any(u => u.Email.ToLower() == s.Email.ToLower() && u.Group.IsHead == true));
                }
                else
                {
                    query = query.Where(s => !_context.Users
                        .Include(u => u.Group)
                        .Any(u => u.Email.ToLower() == s.Email.ToLower() && u.Group.IsHead == true));
                }
            }

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            if (string.IsNullOrEmpty(filter.SortColumn))
            {
                query = query.OrderByDescending(x => x.UpdatedAt);
            }

            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<List<MstSecurityLookUpRead>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.MstSecurities
                .AsNoTracking()
                .Where(fd => fd.Status != 0 && fd.CardNumber != null);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var projected = query.Select(t => new MstSecurityLookUpRead
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
            });
            return await projected.ToListAsync();
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

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            var query = _context.Users
                .Include(u => u.Group)
                .Where(u => u.Id == id && u.Status != 0);
            return await query.FirstOrDefaultAsync() ?? throw new KeyNotFoundException("User not found");
        }

        public async Task AddAsync(MstSecurity security)
        {
            await _context.MstSecurities.AddAsync(security);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MstSecurity security)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(MstSecurity security)
        {
            _context.MstSecurities.Remove(security);
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

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidOrganizationOwnershipAsync(
            Guid organizationId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstOrganization>(
                new[] { organizationId },
                applicationId
            );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidDepartmentOwnershipAsync(
            Guid departmentId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstDepartment>(
                new[] { departmentId },
                applicationId
            );
        }

        public async Task<IReadOnlyCollection<Guid>> CheckInvalidDistrictOwnershipAsync(
            Guid districtId,
            Guid applicationId)
        {
            return await CheckInvalidOwnershipIdsAsync<MstDistrict>(
                new[] { districtId },
                applicationId
            );
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
    }
}
