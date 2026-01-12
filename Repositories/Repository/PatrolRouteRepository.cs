using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;

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

            if (entity == null)
                throw new KeyNotFoundException("PatrolRoute not found");

            if (!isSystemAdmin && entity.ApplicationId != applicationId)
                throw new UnauthorizedAccessException();

            entity.Status = 0;
            await _context.SaveChangesAsync();
        }
    }
}
