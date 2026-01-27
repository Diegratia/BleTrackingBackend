using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Helpers.Consumer.DtoHelpers.MinimalDto;

namespace Repositories.Repository
{
    public class CardAccessRepository : BaseRepository
    {
        public CardAccessRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<CardAccess?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
                .Where(ca => ca.Id == id && ca.Status != 0)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CardAccess>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<CardAccess> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardAccesses
            .Include(ca => ca.CardAccessTimeGroups)
                .ThenInclude(ca => ca.TimeGroup)
            .Include(ca => ca.CardAccessMaskedAreas)
                .ThenInclude(cam => cam.MaskedArea)
            .Where(ca => ca.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<CardAccessMinimalDto> MinimalGetAllQueryableDto()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardAccesses
            .Include(ca => ca.CardAccessTimeGroups)
                .ThenInclude(ca => ca.TimeGroup)
            .Include(ca => ca.CardAccessMaskedAreas)
                .ThenInclude(cam => cam.MaskedArea)
            .Where(ca => ca.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query.Select(ca => new CardAccessMinimalDto
            {
                Id = ca.Id,
                Name = ca.Name,
                AccessNumber = ca.AccessNumber ?? 0,
                AccessScope = ca.AccessScope.ToString(),
                Remarks = ca.Remarks,
                UpdatedAt = ca.UpdatedAt,
                ApplicationId = ca.ApplicationId,

                MaskedAreaIds = ca.CardAccessMaskedAreas
                        .Select(x => (Guid?)x.MaskedAreaId)
                        .ToList(),

                TimeGroupIds = ca.CardAccessTimeGroups
                        .Select(x => (Guid?)x.TimeGroupId)
                        .ToList()
            });

        }
        public async Task<CardAccess> AddAsync(CardAccess entity)
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
                throw new ArgumentException("System Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            _context.CardAccesses.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(CardAccess entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var entity = await _context.CardAccesses.FirstOrDefaultAsync(ca => ca.Id == id && ca.Status != 0);
            if (entity == null)
                throw new KeyNotFoundException("CardAccess not found");

            if (!isSystemAdmin && entity.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            entity.Status = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetCardAccessIdsByLocationAsync(
            List<Guid>? buildingIds,
            List<Guid>? floorIds,
            List<Guid>? floorplanIds)
        {
            var query = _context.CardAccesses
                .Include(ca => ca.CardAccessMaskedAreas)
                    .ThenInclude(cam => cam.MaskedArea)
                    .ThenInclude(ma => ma.Floorplan)
                        .ThenInclude(fp => fp.Floor)
                            .ThenInclude(f => f.Building)
                .Where(ca => ca.Status != 0);

            // 🔹 Filter by building
            if (buildingIds != null && buildingIds.Any())
            {
                query = query.Where(ca =>
                    ca.CardAccessMaskedAreas.Any(cam =>
                        cam.MaskedArea.Floorplan.Floor.Building != null &&
                        buildingIds.Contains(cam.MaskedArea.Floorplan.Floor.Building.Id)
                    ));
            }

            // 🔹 Filter by floor
            if (floorIds != null && floorIds.Any())
            {
                query = query.Where(ca =>
                    ca.CardAccessMaskedAreas.Any(cam =>
                        cam.MaskedArea.Floorplan.Floor != null &&
                        floorIds.Contains(cam.MaskedArea.Floorplan.Floor.Id)
                    ));
            }

            // 🔹 Filter by floorplan
            if (floorplanIds != null && floorplanIds.Any())
            {
                query = query.Where(ca =>
                    ca.CardAccessMaskedAreas.Any(cam =>
                        cam.MaskedArea.Floorplan != null &&
                        floorplanIds.Contains(cam.MaskedArea.Floorplan.Id)
                    ));
            }

            return await query
                .Select(ca => ca.Id)
                .Distinct()
                .ToListAsync();
        }

        
        // non list overload
        // public async Task<List<Guid>> GetCardAccessIdsByLocationAsync(
        //     Guid? buildingId,
        //     Guid? floorId,
        //     Guid? floorplanId)
        // {
        //     var query = _context.CardAccessMaskedAreas.AsQueryable();

        //     if (floorplanId.HasValue)
        //     {
        //         query = query.Where(cam => cam.MaskedArea.FloorplanId == floorplanId.Value);
        //     }
        //     else if (floorId.HasValue)
        //     {
        //         query = query.Where(cam => cam.MaskedArea.Floorplan.FloorId == floorId.Value);
        //     }
        //     else if (buildingId.HasValue)
        //     {
        //         query = query.Where(cam => cam.MaskedArea.Floorplan.Floor.BuildingId == buildingId.Value);
        //     }
        //     else
        //     {
        //         return new List<Guid>(); // no filter
        //     }

        //     return await query
        //         .Select(cam => cam.CardAccessId)
        //         .Distinct()
        //         .ToListAsync();
        // }

    }
}
