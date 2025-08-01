using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;

namespace Repositories.Repository
{
    public abstract class BaseRepository
    {
        protected readonly BleTrackingDbContext _context;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        protected (Guid? ApplicationId, bool IsSystemAdmin) GetApplicationIdAndRole()
        {
            var isSystemAdmin = _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.System.ToString());
            if (isSystemAdmin == true)
            {
                return (null, true); // System admin tidak perlu filter ApplicationId
            }

            // Prioritas 1: Dari token Bearer (claim di JWT)
            var applicationIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("ApplicationId")?.Value;
            if (Guid.TryParse(applicationIdClaim, out var applicationIdFromToken))
            {
                return (applicationIdFromToken, false);
            }

            // Prioritas 2: Dari MstIntegration (X-API-KEY-TRACKING-PEOPLE)
            // var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
            // if (integration?.ApplicationId != null)
            // {
            //     return (integration.ApplicationId, false);
            // }

            return (null, false);
        }

        protected IQueryable<T> ApplyApplicationIdFilter<T>(IQueryable<T> query, Guid? applicationId, bool isSystemAdmin) where T : class, IApplicationEntity
        {
            if (!isSystemAdmin && applicationId.HasValue)
            {
                query = query.Where(e => e.ApplicationId == applicationId);
            }
            return query;
        }

        protected async Task ValidateApplicationIdAsync(Guid applicationId)
        {
            var application = await _context.MstApplications
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.ApplicationStatus != 0);
            if (application == null)
                throw new ArgumentException($"Application with ID {applicationId} not found.");
        }

        protected void ValidateApplicationIdForEntity<T>(T entity, Guid? applicationId, bool isSystemAdmin) where T : class, IApplicationEntity
        {
            if (!isSystemAdmin && applicationId.HasValue && entity.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("ApplicationId mismatch");
        }
    }
}