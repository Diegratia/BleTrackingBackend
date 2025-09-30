using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;

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
            .Include(ca => ca.CardAccessMaskedAreas)
                .ThenInclude(cam => cam.MaskedArea)
            .Where(ca => ca.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
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
    }
}
