using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore.Storage;

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
                return (null, true); // system ga perlu filter applicationId
            }

            // cek di token
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId")?.Value;
            if (Guid.TryParse(applicationIdClaim, out var applicationIdFromToken))
            {
                return (applicationIdFromToken, false);
            }
            // var integrationFromHeader = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
            // if (integrationFromHeader?.ApplicationId != null)
            // {
            //     return (integrationFromHeader.ApplicationId, false);
            // }
            var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
            if (integration?.ApplicationId != null)
            {
                return (integration.ApplicationId, false);
            }

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

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            IDbContextTransaction transaction = null;
            if (_context.Database.CurrentTransaction == null)
            {
                transaction = await _context.Database.BeginTransactionAsync();
            }

            try
            {
                await action();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }


        protected string GetUserEmail()
        {
            var email = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedAccessException("User email claim is missing.");
            return email;
        }


        protected bool IsSuperAdmin()
        {
            return _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.SuperAdmin.ToString()) ?? false;
        }

        protected bool IsPrimary()
        {
            return _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.Primary.ToString()) ?? false;
        }

        protected bool IsPrimaryAdmin()
        {
            return _httpContextAccessor.HttpContext?.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == LevelPriority.PrimaryAdmin.ToString()) ?? false;
        }
        


    }
}

