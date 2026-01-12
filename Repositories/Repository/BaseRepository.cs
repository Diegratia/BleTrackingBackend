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
using Repositories.TenantContexts;

namespace Repositories.Repository
{
    public abstract class BaseRepository
    {
        protected readonly BleTrackingDbContext _context;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        // protected readonly TenantContext _tenantContext;

        protected BaseRepository(BleTrackingDbContext context,
         IHttpContextAccessor httpContextAccessor
        //   TenantContext tenantContext
          )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            // _tenantContext = tenantContext;
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

        /// <summary>
        /// Query default untuk entity bertenant
        /// </summary>
        // protected IQueryable<T> Query<T>() where T : class, IApplicationEntity
        // {
        //     var query = _context.Set<T>().AsQueryable();

        //     if (!_tenantContext.IsSystemAdmin)
        //         query = query.Where(x => x.ApplicationId == _tenantContext.ApplicationId);

        //     return query;
        // }

        // /// <summary>
        // /// Dipanggil sebelum INSERT / UPDATE
        // /// </summary>
        // protected void PrepareForSave<T>(T entity) where T : class, IApplicationEntity
        // {
        //     if (!_tenantContext.IsSystemAdmin)
        //     {
        //         entity.ApplicationId = _tenantContext.ApplicationId!.Value;
        //     }
        //     // SystemAdmin: ApplicationId HARUS sudah di-set dari service
        // }


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

        protected (DateTime from, DateTime to)? ResolveTimeRange(string? timeReport)
        {
            if (string.IsNullOrWhiteSpace(timeReport))
                return null;

            var now = DateTime.UtcNow;

            return timeReport.Trim().ToLower() switch
            {
                "daily" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "weekly" => (now.Date.AddDays(-(int)now.DayOfWeek + 1),
                            now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1)),
                "monthly" => (new DateTime(now.Year, now.Month, 1),
                            new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
                                .AddDays(1).AddTicks(-1)),
                "yearly" => (new DateTime(now.Year, 1, 1),
                            new DateTime(now.Year, 12, 31).AddDays(1).AddTicks(-1)),
                _ => null
            };
        }
    }
}

