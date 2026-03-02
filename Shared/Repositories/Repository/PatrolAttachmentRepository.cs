using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class PatrolAttachmentRepository : BaseRepository
    {
        public PatrolAttachmentRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public IQueryable<PatrolCaseAttachment> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolCaseAttachments
                .Include(d => d.PatrolCase)
                .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<PatrolAttachmentRead> ProjectToRead(IQueryable<PatrolCaseAttachment> query)
        {
            return query.Select(x => new PatrolAttachmentRead
            {
                Id = x.Id,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt,
                Status = x.Status,
                ApplicationId = x.ApplicationId,
                PatrolCaseId = x.PatrolCaseId,
                FileUrl = x.FileUrl,
                FileType = x.FileType,
                MimeType = x.MimeType,
                UploadedAt = x.UploadedAt
            });
        }

        public async Task<PatrolAttachmentRead?> GetByIdAsync(Guid id)
        {
            return await ProjectToRead(BaseEntityQuery())
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PatrolCaseAttachment?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<PatrolAttachmentRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<PatrolAttachmentRead> Data, int Total, int Filtered)> FilterAsync(
            PatrolAttachmentFilter filter)
        {
            var query = BaseEntityQuery();

            if (filter.PatrolCaseId.HasValue)
                query = query.Where(x => x.PatrolCaseId == filter.PatrolCaseId.Value);

            if (filter.FileType.HasValue)
                query = query.Where(x => x.FileType == filter.FileType.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x => x.FileUrl != null && x.FileUrl.ToLower().Contains(search));
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

        public async Task AddAsync(PatrolCaseAttachment attachment)
        {
            await _context.PatrolCaseAttachments.AddAsync(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PatrolCaseAttachment attachment)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PatrolCaseAttachment attachment)
        {
            _context.PatrolCaseAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }

        public IQueryable<PatrolCaseAttachment> GetAllQueryable()
        {
            return BaseEntityQuery();
        }

        public async Task<List<PatrolAttachmentRead>> GetByPatrolCaseIdAsync(Guid caseId)
        {
            return await ProjectToRead(BaseEntityQuery())
                .Where(x => x.PatrolCaseId == caseId)
                .ToListAsync();
        }

        public async Task<bool> CaseExistsAsync(Guid caseId)
        {
            return await _context.PatrolCases
                .AnyAsync(f => f.Id == caseId && f.Status != 0);
        }
    }
}
