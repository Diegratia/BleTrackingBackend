using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository
{
    public class CardRepository : BaseRepository
    {
        public CardRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<Card?> GetByIdAsync(Guid id)
        {

            return await GetAllQueryable()
            .Where(b => b.Id == id && b.StatusCard != 0)
            .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Card>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<Card> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Cards
                .Include(b => b.RegisteredMaskedArea)
                .Include(b => b.Member)
                .Include(b => b.Visitor)
                .Include(b => b.Application)
                .Where(b => b.StatusCard != 0);

                query = query.WithActiveRelations();

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<Card> AddAsync(Card card)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin user.");

                card.ApplicationId = applicationId.Value;
            }
            else if (card.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System Admin must specify ApplicationId explicitly.");
            }

            await ValidateApplicationIdAsync(card.ApplicationId);
            ValidateApplicationIdForEntity(card, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(card, applicationId, isSystemAdmin);

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task UpdateAsync(Card card)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(card.ApplicationId);
            ValidateApplicationIdForEntity(card, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(card, applicationId, isSystemAdmin);

            // _context.Cards.Update(card);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var card = await _context.Cards.FirstOrDefaultAsync(b => b.Id == id && b.StatusCard != 1);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            if (!isSystemAdmin && card.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this entity.");

            await _context.SaveChangesAsync();
        }

        public async Task<FloorplanMaskedArea?> GetMaskedAreaByIdAsync(Guid id)
        {
            return await _context.FloorplanMaskedAreas
                .WithActiveRelations()
                .FirstOrDefaultAsync(b => b.Id == id && b.Status != 0);
        }

        public async Task<IEnumerable<Card>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        private async Task ValidateRelatedEntitiesAsync(Card card, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            if (card.MemberId.HasValue)
            {
                var member = await _context.MstMembers
                    .WithActiveRelations()
                    .FirstOrDefaultAsync(m => m.Id == card.MemberId && m.ApplicationId == applicationId);

                if (member == null)
                    throw new UnauthorizedAccessException("Member not found or not accessible in your application.");
            }

            if (card.VisitorId.HasValue)
            {
                var visitor = await _context.Visitors
                    .WithActiveRelations()
                    .FirstOrDefaultAsync(v => v.Id == card.VisitorId && v.ApplicationId == applicationId);

                if (visitor == null)
                    throw new UnauthorizedAccessException("Visitor not found or not accessible in your application.");
            }
        }
    }
}
