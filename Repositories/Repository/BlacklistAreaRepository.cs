using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer.DtoHelpers;
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
    public class BlacklistAreaRepository : BaseRepository
    {
        public BlacklistAreaRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

            public async Task<int> GetCountAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.BlacklistAreas
                .AsNoTracking()
                .Where(c => c.Status != 0);
            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);
            
            var visitorIds = await q
                .Where(x => x.VisitorId != null)
                .Select(x => x.VisitorId!.Value)
                .Distinct()
                .ToListAsync();

            var memberIds = await q
                .Where(x => x.MemberId != null)
                .Select(x => x.MemberId!.Value)
                .Distinct()
                .ToListAsync();
            
            var totalUnique = visitorIds
                .Union(memberIds)
                .Distinct()
                .Count();
            

            return totalUnique;
        }

         public async Task<List<BlacklistRM>> GetTopBlacklistAsync(int topCount = 5)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var q = _context.BlacklistAreas
                .AsNoTracking()
                .Where(c => c.Status != 0);

            q = ApplyApplicationIdFilter(q, applicationId, isSystemAdmin);

            return await q
                .OrderByDescending(x => x.UpdatedAt) 
                .Take(topCount)
                .Select(x => new BlacklistRM
                {
                    Id = x.Id,
                    BlacklistPersonName = x.Visitor.Name ?? x.Member.Name ?? "Unknown Person", 
                })
                .ToListAsync();
        }

        public async Task<List<BlacklistArea>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<BlacklistArea?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.BlacklistAreas     
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .Include(v => v.Member)
                .Where(v => v.Id == id && v.Status != 0);
            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public IQueryable<BlacklistArea> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.BlacklistAreas
                .IgnoreQueryFilters()
                .Include(v => v.FloorplanMaskedArea)
                .Include(v => v.Visitor)
                .Include(v => v.Member)
                .Where(v => v.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        //  public IQueryable<BlacklistAreaDtoMinimal> GetAllQueryableMinimal()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.BlacklistAreas
        //         .Include(v => v.FloorplanMaskedArea)
        //         .Include(v => v.Visitor)
        //         .AsQueryable();

        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

        //     return query.Select(v => new BlacklistAreaDtoMinimal
        //     {
        //         Id = v.Id,
        //         FloorplanMaskedArea = v.FloorplanMaskedArea == null ? null : new FloorplanMaskedAreaDtoMinimal
        //         {
        //             Id = v.FloorplanMaskedArea.Id,
        //             Name = v.FloorplanMaskedArea.Name
        //         },
        //         Visitor = v.Visitor == null ? null : new VisitorDtoMinimal
        //         {
        //             Id = v.Visitor.Id,
        //             Name = v.Visitor.Name 
        //         }
        //     });
        // }

        public async Task AddRangeAsync(IEnumerable<BlacklistArea> entities)
        {
             var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();


            foreach (var entity in entities)
            {
         
                if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                entity.ApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }
                await ValidateApplicationIdAsync(entity.ApplicationId);
                ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);
                await ValidateRelatedEntitiesAsync(entity, applicationId, isSystemAdmin);
            }

            await _context.BlacklistAreas.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(BlacklistArea entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                entity.ApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(entity, applicationId, isSystemAdmin);

            await _context.BlacklistAreas.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BlacklistArea entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(entity, applicationId, isSystemAdmin);

            // _context.BlacklistAreas.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(BlacklistArea entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && entity.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            _context.BlacklistAreas.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private async Task ValidateRelatedEntitiesAsync(BlacklistArea entity, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            // var visitor = await _context.Visitors
            //     .FirstOrDefaultAsync(v => v.Id == entity.VisitorId && v.ApplicationId == applicationId);

            // if (visitor == null)
            //     throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");

            var floorplanArea = await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(f => f.Id == entity.FloorplanMaskedAreaId && f.ApplicationId == applicationId);

            if (floorplanArea == null)
                throw new UnauthorizedAccessException("FloorplanMaskedArea not found or not accessible in your application.");
        }
    }
}
