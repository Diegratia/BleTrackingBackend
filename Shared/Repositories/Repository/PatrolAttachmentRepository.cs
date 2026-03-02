using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;

namespace Repositories.Repository
{
    public class PatrolAttachmentRepository : BaseRepository
    {
        public PatrolAttachmentRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<PatrolCaseAttachment?> GetByIdAsync(Guid id)
        {
            return await GetAllQueryable()
           .Where(a => a.Id == id && a.Status != 0)
           .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatrolCaseAttachment>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<PatrolCaseAttachment> AddAsync(PatrolCaseAttachment attachment)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                attachment.ApplicationId = applicationId.Value;
            }
            else if (attachment.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(attachment.ApplicationId);
            ValidateApplicationIdForEntity(attachment, applicationId, isSystemAdmin);

            _context.PatrolCaseAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task UpdateAsync(PatrolCaseAttachment attachment)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(attachment.ApplicationId);
            ValidateApplicationIdForEntity(attachment, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolCaseAttachments
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var attachment = await query.FirstOrDefaultAsync();

            if (attachment == null)
                return;

            await _context.SaveChangesAsync();
        }


        public IQueryable<PatrolCaseAttachment> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolCaseAttachments
            .Include(d => d.PatrolCase)
            .Where(d => d.Status != 0);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }
        
            public async Task<List<PatrolCaseAttachment>> GetByPatrolCaseIdAsync(Guid caseId)
        {
            return await _context.PatrolCaseAttachments
                .Where(ma => ma.PatrolCaseId == caseId && ma.Status != 0)
                .ToListAsync();
        }

        public async Task<bool> CaseExistsAsync(Guid caseId)
        {
            return await _context.PatrolCases
                .AnyAsync(f => f.Id == caseId && f.Status != 0);
        }

    }
}
