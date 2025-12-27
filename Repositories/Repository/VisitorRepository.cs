using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class VisitorRepository : BaseRepository
    {
        public VisitorRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }


        public async Task<List<Visitor>> GetAllAsync()
        {
            return await GetAllQueryable().AsNoTracking().ToListAsync();
        }

        public async Task<Visitor?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
            .Where(x => x.Id == id && x.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<Visitor?> GetByIdPublicAsync(Guid id)
        {
            return await _context.Visitors
            .Where(x => x.Id == id && x.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<Visitor?> GetByIdPublicDuplicateAsync(string email, string identityId, string personId, Guid id)
        {
            return await _context.Visitors
            .Where(x => x.Id != id && x.Status != 0)
               .FirstOrDefaultAsync(b =>
                        (!string.IsNullOrWhiteSpace(email) && b.Email == email) ||
                        (!string.IsNullOrWhiteSpace(identityId) && b.IdentityId == identityId) ||
                        (!string.IsNullOrWhiteSpace(personId) && b.PersonId == personId)
                    );
        }

       public async Task AddAsync(Visitor visitor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");

                visitor.ApplicationId = applicationId.Value;
            }
            else if (visitor.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(visitor.ApplicationId);
            ValidateApplicationIdForEntity(visitor, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(visitor, applicationId, isSystemAdmin);

            await _context.Visitors.AddAsync(visitor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Visitor visitor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(visitor.ApplicationId);
            ValidateApplicationIdForEntity(visitor, applicationId, isSystemAdmin);
            // await ValidateRelatedEntitiesAsync(visitor, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsyncRaw(Visitor visitor)
        {
            // await ValidateRelatedEntitiesAsync(visitor, applicationId, isSystemAdmin);
            await _context.SaveChangesAsync();
        }



  

        public async Task DeleteAsync(Visitor visitor)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && visitor.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("Cannot delete visitor from a different application.");

            visitor.Status = 0;
            await _context.SaveChangesAsync();
        }

        // public async Task<bool> ApplicationExists(Guid id)
        // {
        //     return await _context.MstApplications.AnyAsync(f => f.Id == id);
        // }

        public IQueryable<Visitor> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Visitors
                // .Include(x => x.Department)
                // .Include(x => x.District)
                // .Include(x => x.Organization)
                // .Include(v => v.Application)
                .Where(x => x.Status != 0);

                query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

            public IQueryable<Visitor> GetAllQueryableRaw()
        {

            var query = _context.Visitors
                // .Include(x => x.Department)
                // .Include(x => x.District)
                // .Include(x => x.Organization)
                // .Include(v => v.Application)
                .Where(x => x.Status != 0);

                query = query.WithActiveRelations();

            return query;
        }

        //   public IQueryable<Visitor> GetAllQueryable()
        // {
        //     var userEmail = GetUserEmail();
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        //     var isSuperAdmin = IsSuperAdmin();

        //     var query = _context.Visitors.AsQueryable();

        //     if (!isSystemAdmin && !isSuperAdmin)
        //     {
        //         if (!string.IsNullOrWhiteSpace(userEmail))
        //         {
        //             var userRole = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        //             if (userRole == LevelPriority.Secondary.ToString())
        //             {
        //                 query = query.Where(v =>
        //                     _context.TrxVisitors.Any(t =>
        //                         _context.MstMembers.Any(m => m.Email == userEmail && t.PurposePerson == m.Id) &&
        //                         t.VisitorId == v.Id));
        //             }
        //             else if (userRole == LevelPriority.UserCreated.ToString())
        //             {
        //                 query = query.Where(v => v.Email == userEmail);
        //             }
        //             else
        //             {
        //                 query = query.Where(v => false); // No access for other roles
        //             }
        //         }
        //         else
        //         {
        //             query = query.Where(v => false); // No access if email is missing
        //         }
        //     }

        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        //     return query;
        // }

        public async Task<IEnumerable<Visitor>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

          public async Task<Visitor> GetByEmailAsync(string email)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.Visitors
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
            return await query.FirstOrDefaultAsync();
        }

                  public async Task<Visitor> GetByEmailAsyncRaw(string email)
        {
            var query = _context.Visitors
                .Where(u => u.Email.ToLower() == email.ToLower() && u.Status != 0);
            return await query.FirstOrDefaultAsync();
        }

        //  public async Task<Visitor> GetByEmailConfirmPasswordAsyncRaw(string email)
        // {
        //     return await _context.Visitors
        //     .FirstOrDefaultAsync(u => u.Email == email);
        // }


        private async Task ValidateRelatedEntitiesAsync(Visitor visitor, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return; // admin bebas input apa aja

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin user.");

            // // Cek Department
            // var department = await _context.MstDepartments
            //     .Where(d => d.Id == visitor.DepartmentId && d.ApplicationId == applicationId)
            //     .FirstOrDefaultAsync();
            // if (department == null)
            //     throw new UnauthorizedAccessException("Invalid DepartmentId for this application.");

            // // Cek District
            // var district = await _context.MstDistricts
            //     .Where(d => d.Id == visitor.DistrictId && d.ApplicationId == applicationId)
            //     .FirstOrDefaultAsync();
            // if (district == null)
            //     throw new UnauthorizedAccessException("Invalid DistrictId for this application.");

            // // Cek Organization
            // var organization = await _context.MstOrganizations
            //     .Where(o => o.Id == visitor.OrganizationId && o.ApplicationId == applicationId)
            //     .FirstOrDefaultAsync();
            // if (organization == null)
            //     throw new UnauthorizedAccessException("Invalid OrganizationId for this application.");
        }

    }
}
