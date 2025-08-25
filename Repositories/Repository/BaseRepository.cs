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

            // Prioritas 2: Dari MstIntegration (X-API-KEY-TRACKING-PEOPLE)
            var integration = _httpContextAccessor.HttpContext?.Items["MstIntegration"] as MstIntegration;
            if (integration?.ApplicationId != null)
            {
                return (integration.ApplicationId, false);
            }

            // var referer = _httpContextAccessor.HttpContext.Request.Headers["Referer"].FirstOrDefault();
            // if (referer != null && !referer.StartsWith("https://domain-test.com"))
            // {
            //     // Return error atau tidak mengizinkan akses
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

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
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
        
//         var apiKey = _httpContextAccessor.HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
// var domain = _httpContextAccessor.HttpContext.Request.Headers["Referer"].FirstOrDefault();

// var validDomain = GetValidDomainFromApiKey(apiKey);

// if (validDomain != null && !domain.StartsWith(validDomain))
// {
//     // Return error atau tidak mengizinkan akses
// }

// // ...

// private string GetValidDomainFromApiKey(string apiKey)
// {
//     // Query tabel untuk mendapatkan domain yang diizinkan untuk API key
//     var domain = _dbContext.ApiKeys.Where(x => x.ApiKey == apiKey).Select(x => x.Domain).FirstOrDefault();
//     return domain;
// }
        
        // public enum LevelPriority
        // {
        //     System,
        //     SuperAdmin,
        //     PrimaryAdmin,
        //     Primary,
        //     Secondary,
        //     UserCreated
        // }

        //     public static IQueryable<Card> WithActiveRelations(this IQueryable<Card> query)
        //     {
        //         return query.Where(c =>
        //             (c.Visitor == null || c.Visitor.Status != 0) &&
        //             (c.Visitor.Department == null || c.Visitor.Department.Status != 0) &&
        //             (c.Visitor.Department.District == null || c.Visitor.Department.District.Status != 0));
        //     }

        // public static IQueryable<FloorplanMaskedArea> WithActiveRelations(this IQueryable<FloorplanMaskedArea> query)
        // {
        //     return query.Where(m =>
        //         m.Floorplan != null &&
        //         m.Floorplan.Status != 0 &&
        //         m.Floorplan.Floor != null &&
        //         m.Floorplan.Floor.Status != 0 &&
        //         m.Floorplan.Floor.Department != null &&
        //         m.Floorplan.Floor.Department.Status != 0);
        // }


    }
}

