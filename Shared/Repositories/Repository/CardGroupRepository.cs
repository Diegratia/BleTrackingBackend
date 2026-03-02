using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class CardGroupRepository : BaseRepository
    {
        public CardGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<CardGroup> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardGroups
                .Include(b => b.Application)
                .Include(b => b.Cards)
                .Where(b => b.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<CardGroupRead> ProjectToRead(IQueryable<CardGroup> query)
        {
            return query.Select(x => new CardGroupRead
            {
                Id = x.Id,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
                Status = x.Status,
                ApplicationId = x.ApplicationId,
                Name = x.Name,
                Remarks = x.Remarks
            });
        }

        public async Task<CardGroupRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CardGroup?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .Include(cg => cg.Cards)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<CardGroupRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<CardGroupRead> Data, int Total, int Filtered)> FilterAsync(
            CardGroupFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x => x.Name != null && x.Name.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            if (string.IsNullOrEmpty(filter.SortColumn))
            {
                query = query.OrderByDescending(x => x.UpdatedAt);
            }

            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task AddAsync(CardGroup cardGroup)
        {
            await _context.CardGroups.AddAsync(cardGroup);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CardGroup cardGroup)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CardGroup cardGroup)
        {
            _context.CardGroups.Remove(cardGroup);
            await _context.SaveChangesAsync();
        }

        public IQueryable<CardGroup> GetAllQueryable()
        {
            return BaseEntityQuery();
        }

        public async Task<IEnumerable<CardGroup>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }
    }
}
