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
    public class CardGroupRepository : BaseRepository
    {
        public CardGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<CardGroup?> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(b => b.Id == id && b.Status != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CardGroup>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync() ?? null;
        }

        
        public IQueryable<CardGroup> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardGroups
                .Include(b => b.Application)
                .Include(b => b.Cards)
               .Include(b => b.CardGroupCardAccesses)
                        .ThenInclude(cga => cga.CardAccess)
                .Where(b => b.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<CardGroup> AddAsync(CardGroup cardGroup)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                cardGroup.ApplicationId = applicationId.Value;
            }
            else if (cardGroup.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(cardGroup.ApplicationId);
            ValidateApplicationIdForEntity(cardGroup, applicationId, isSystemAdmin);
            // await ValidateRelatedEntitiesAsync(cardGroup, applicationId, isSystemAdmin);

            _context.CardGroups.Add(cardGroup);
            await _context.SaveChangesAsync();
            return cardGroup;
        }

        public async Task UpdateAsync(CardGroup cardGroup)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(cardGroup.ApplicationId);
            ValidateApplicationIdForEntity(cardGroup, applicationId, isSystemAdmin);
            // await ValidateRelatedEntitiesAsync(cardGroup, applicationId, isSystemAdmin);

            // _context.Cards.Update(cardGroup);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var cardGroup = await _context.CardGroups.FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
            if (cardGroup == null)
                throw new KeyNotFoundException("CardGroup not found");

            if (!isSystemAdmin && cardGroup.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CardGroup>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        // private async Task ValidateRelatedEntitiesAsync(CardGroup cardGroup, Guid? applicationId, bool isSystemAdmin)
        // {
        //     if (isSystemAdmin) return;

        //     if (!applicationId.HasValue)
        //         throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

        //     if (cardGroup.MemberId.HasValue)
        //     {
        //         var member = await _context.MstMembers
        //             .WithActiveRelations()
        //             .FirstOrDefaultAsync(m => m.Id == cardGroup.MemberId && m.ApplicationId == applicationId);

        //         if (member == null)
        //             throw new UnauthorizedAccessException("Member not found or not accessible in your application.");
        //     }

        //     if (cardGroup.VisitorId.HasValue)
        //     {
        //         var visitor = await _context.Visitors
        //             .WithActiveRelations()
        //             .FirstOrDefaultAsync(v => v.Id == cardGroup.VisitorId && v.ApplicationId == applicationId);

        //         if (visitor == null)
        //             throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");
        //     }
        // }
    }
}
