using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Repositories.Repository.RepoModel;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class PatrolRouteRepository : BaseRepository
    {
        public PatrolRouteRepository(
            BleTrackingDbContext context,
            IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<PatrolRoute> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolRoutes
                .Include(x => x.PatrolRouteAreas)
                .ThenInclude(x => x.PatrolArea)
                .Where(x => x.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<PatrolRouteLookUpRM> MinimalGetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolRoutes
            .Include(x => x.PatrolRouteAreas)
            .Where(ca => ca.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query.Select(ca => new PatrolRouteLookUpRM
            {
                Id = ca.Id,
                Name = ca.Name,
                Description = ca.Description,

                PatrolAreaIds = ca.PatrolRouteAreas
                        .Select(x => (Guid?)x.PatrolAreaId)
                        .ToList()
            });
        }
        public async Task<List<PatrolRouteLookUpRM>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolRoutes
            .Include(x => x.PatrolRouteAreas)
            .AsNoTracking()
            .Where(ca => ca.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var projected = query.Select(ca => new PatrolRouteLookUpRM
            {
                Id = ca.Id,
                Name = ca.Name,
                Description = ca.Description,

                PatrolAreaIds = ca.PatrolRouteAreas
                        .Select(x => (Guid?)x.PatrolAreaId)
                        .ToList()
            });
            return await projected.ToListAsync();
        }

        // public async Task<List<MstSecurityLookUpRM>> GetAllLookUpAsync()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        //     var query = _context.MstSecurities
        //     .AsNoTracking()
        //     .Where(fd => fd.Status != 0 && fd.CardNumber != null);

        //     var projected = query.Select(t => new MstSecurityLookUpRM
        //     {
        //         Id = t.Id,
        //         Name = t.Name,
        //         PersonId = t.PersonId,
        //         CardNumber = t.CardNumber,
        //         OrganizationId = t.OrganizationId,
        //         DepartmentId = t.DepartmentId,
        //         DistrictId = t.DistrictId,
        //         OrganizationName = t.Organization.Name,
        //         DepartmentName = t.Department.Name,
        //         DistrictName = t.District.Name,
        //         ApplicationId = t.ApplicationId
        //     }); 
        //     return await projected.ToListAsync();
        // }
        

        public async Task<PatrolRoute?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<PatrolRoute?>> GetAllAsync()
        
        {
            return await GetAllQueryable().ToListAsync();
        }
        

        public async Task<PatrolRoute> AddAsync(PatrolRoute entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required.");
                entity.ApplicationId = applicationId.Value;
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            _context.PatrolRoutes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var entity = await _context.PatrolRoutes
                .FirstOrDefaultAsync(x => x.Id == id && x.Status != 0);

            entity.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}
