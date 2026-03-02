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
using System.Linq.Dynamic.Core;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class PatrolCaseRepository : BaseRepository
    {
        public PatrolCaseRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        private IQueryable<PatrolCase> BaseEntityQuery()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolCases
                .Where(x => x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            // {
            //     query = query.Where(pas =>
            //             pas.Security != null
            //             && pas.Security.Email == userEmail
            //         );
            // }

            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pc =>
                    (pc.Security != null && pc.Security.Email == userEmail)
                    || _context.PatrolAssignmentSecurities.Any(pas =>
                        pas.PatrolAssignmentId == pc.PatrolAssignmentId
                        && pas.Security != null
                        && pas.Security.Email == userEmail
                    )
                );
            }


            return query;
        }


        public async Task<PatrolCaseRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();

        }

        // For approval operations - allows PrimaryAdmin to access all cases
        public async Task<PatrolCaseRead?> GetByIdForApprovalAsync(Guid id)
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolCases
                .Where(x => x.Id == id && x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // For PrimaryAdmin+, allow access to all cases for approval
            // For Primary (creators), only allow access to their own cases
            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pc =>
                    (pc.Security != null && pc.Security.Email == userEmail)
                    || _context.PatrolAssignmentSecurities.Any(pas =>
                        pas.PatrolAssignmentId == pc.PatrolAssignmentId
                        && pas.Security != null
                        && pas.Security.Email == userEmail
                    )
                );
            }

            return await ProjectToRead(query).FirstOrDefaultAsync();
        }
        public async Task<PatrolCase?> GetByIdEntityAsync(Guid id)
        {
            var query = BaseEntityQuery()
            .Include(x => x.PatrolCaseAttachments)
            .Where(x => x.Id == id);
            return await query.FirstOrDefaultAsync();
        }

        // For approval operations - allows PrimaryAdmin to access all cases
        public async Task<PatrolCase?> GetByIdEntityForApprovalAsync(Guid id)
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolCases
                .Include(x => x.PatrolCaseAttachments)
                .Where(x => x.Id == id && x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // For PrimaryAdmin+, allow access to all cases for approval
            // For Primary (creators), only allow access to their own cases
            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(pc =>
                    (pc.Security != null && pc.Security.Email == userEmail)
                    || _context.PatrolAssignmentSecurities.Any(pas =>
                        pas.PatrolAssignmentId == pc.PatrolAssignmentId
                        && pas.Security != null
                        && pas.Security.Email == userEmail
                    )
                );
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PatrolCaseRead>> GetAllAsync()
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).ToListAsync();
        }

        public async Task<PatrolCase> AddAsync(PatrolCase patrolCase)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                patrolCase.ApplicationId = applicationId.Value;
            }
            else if (patrolCase.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(patrolCase.ApplicationId);
            ValidateApplicationIdForEntity(patrolCase, applicationId, isSystemAdmin);

            _context.PatrolCases.Add(patrolCase);

            await _context.SaveChangesAsync();
            return patrolCase;
        }

        public async Task UpdateAsync(PatrolCase patrolCase)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolCase.ApplicationId);
            ValidateApplicationIdForEntity(patrolCase, applicationId, isSystemAdmin);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolCases
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var patrolCase = await query.FirstOrDefaultAsync();

            if (patrolCase == null)
                return;

            await _context.SaveChangesAsync();
        }

        public IQueryable<PatrolCaseRead> ProjectToRead(
            IQueryable<PatrolCase> query
        )
        {
            var projected = query.AsNoTracking().Select(t => new PatrolCaseRead
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                CaseType = t.CaseType,
                ThreatLevel = t.ThreatLevel,
                CaseStatus = t.CaseStatus,
                PatrolSessionId = t.PatrolSessionId,
                SecurityId = t.SecurityId,
                SecurityHead1Id = t.SecurityHead1Id,
                SecurityHead2Id = t.SecurityHead2Id,
                ApprovalType = t.ApprovalType,
                ApprovedByHead1Id = t.ApprovedByHead1Id,
                ApprovedByHead1Name = t.SecurityHead1 == null ? null : t.SecurityHead1.Name,
                ApprovedByHead1Identity = t.SecurityHead1 == null ? null : t.SecurityHead1.IdentityId,
                ApprovedByHead2Id = t.ApprovedByHead2Id,
                ApprovedByHead2Name = t.SecurityHead2 == null ? null : t.SecurityHead2.Name,
                ApprovedByHead2Identity = t.SecurityHead2 == null ? null : t.SecurityHead2.IdentityId,
                ApprovedByHead1At = t.ApprovedByHead1At,
                ApprovedByHead2At = t.ApprovedByHead2At,
                PatrolAssignmentId = t.PatrolAssignmentId,
                PatrolRouteId = t.PatrolRouteId,
                ApplicationId = t.ApplicationId,
                Security = t.Security == null ? null : new MstSecurityLookUpRead
                {
                    Id = t.Security.Id,
                    Name = t.Security.Name,
                    PersonId = t.Security.PersonId,
                    CardNumber = t.Security.CardNumber,
                    OrganizationId = t.Security.OrganizationId,
                    DepartmentId = t.Security.DepartmentId,
                    DistrictId = t.Security.DistrictId,
                    OrganizationName = t.Security.Organization.Name,
                    DepartmentName = t.Security.Department.Name,
                    DistrictName = t.Security.District.Name,
                },
                PatrolAssignment = t.PatrolAssignment == null ? null : new PatrolAssignmentLookUpRead
                {
                    Id = t.PatrolAssignment.Id,
                    Name = t.PatrolAssignment.Name,
                    Description = t.PatrolAssignment.Description,
                },
                PatrolRoute = t.PatrolRoute == null ? null : new PatrolRouteLookUpRead
                {
                    Id = t.PatrolRoute.Id,
                    Name = t.PatrolRoute.Name,
                    Description = t.PatrolRoute.Description,
                    StartAreaName = t.PatrolRoute.PatrolRouteAreas
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault(),
                    EndAreaName = t.PatrolRoute.PatrolRouteAreas
                        .OrderByDescending(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault()
                },
                Attachments = t.PatrolCaseAttachments.Select(x => new PatrolAttachmentRead
                {
                    Id = x.Id,
                    FileUrl = x.FileUrl,
                    FileType = x.FileType,
                    MimeType = x.MimeType,
                    UploadedAt = x.UploadedAt
                }).ToList()
            });

            return projected;
        }
        

        public async Task<(List<PatrolCaseRead> Data, int Total, int Filtered)> FilterAsync(
            PatrolCaseFilter filter
        )
        {
            var query = BaseEntityQuery();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Title.ToLower().Contains(search) ||
                    x.Description.ToLower().Contains(search)
                );
            }

            if (filter.CaseType.HasValue)
                query = query.Where(x => x.CaseType == filter.CaseType.Value);

            if (filter.CaseStatus.HasValue)
                query = query.Where(x => x.CaseStatus == filter.CaseStatus.Value);

            // Use ExtractIds for ID filters (supports both single Guid and Guid array)
            var securityIds = ExtractIds(filter.SecurityId);
            if (securityIds.Count > 0)
                query = query.Where(x => x.SecurityId.HasValue && securityIds.Contains(x.SecurityId.Value));

            var assignmentIds = ExtractIds(filter.PatrolAssignmentId);
            if (assignmentIds.Count > 0)
                query = query.Where(x => x.PatrolAssignmentId.HasValue && assignmentIds.Contains(x.PatrolAssignmentId.Value));

            var routeIds = ExtractIds(filter.PatrolRouteId);
            if (routeIds.Count > 0)
                query = query.Where(x => x.PatrolRouteId.HasValue && routeIds.Contains(x.PatrolRouteId.Value));

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.UpdatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.UpdatedAt <= filter.DateTo.Value);

            var total = await query.CountAsync();
            var filtered = await query.CountAsync();

            // Sorting & Paging
            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            // Use ProjectToRead for single source of truth
            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }


        public async Task<PatrolSession?> GetPatrolSessionAsync(Guid sessionId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolSessions
                .Include(x => x.PatrolAssignment)
                .Include(x => x.PatrolRoute)
                .Where(x => x.Id == sessionId && x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<MstSecurity?> GetIdBySecurityEmailAsync(string email)
        {
            return await _context.MstSecurities.FirstOrDefaultAsync(f => f.Email == email && f.Status != 0);
        }

        public async Task<Guid> GetSecurityIdByEmailAsync(string email)
        {
            var security = await _context.MstSecurities
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Email == email && s.Status != 0);

            if (security == null)
                throw new Exception($"Security with email {email} not found");

            return security.Id;
        }

        public async Task<MstSecurity?> GetSecurityByIdAsync(Guid securityId)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.MstSecurities
                .AsNoTracking()
                .Where(s => s.Id == securityId && s.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> SessionExistsAsync(Guid sessionId)
        {
            return await _context.PatrolSessions
                .AnyAsync(f => f.Id == sessionId && f.Status != 0);
        }
        public async Task AddManyAsync(IEnumerable<PatrolCaseAttachment> attachments)
        {
            _context.PatrolCaseAttachments.AddRange(attachments);
            await _context.SaveChangesAsync();
        }
        
        public async Task RemoveAllAttachmentsByCaseIdAsync(Guid caseId)
        {
            await _context.PatrolCaseAttachments
                .Where(x => x.PatrolCaseId == caseId)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// Check if a security is member of an assignment group
        /// </summary>
        public async Task<bool> IsSecurityInAssignmentAsync(
            Guid securityId,
            Guid assignmentId)
        {
            return await _context.PatrolAssignmentSecurities
                .AsNoTracking()
                .AnyAsync(pas =>
                    pas.PatrolAssignmentId == assignmentId
                    && pas.SecurityId == securityId
                    && pas.Status != 0
                );
        }

        /// <summary>
        /// Get attachment by ID with authorization check
        /// </summary>
        public async Task<PatrolCaseAttachment?> GetAttachmentByIdAsync(
            Guid caseId,
            Guid attachmentId)
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolCaseAttachments
                .Where(x => x.Id == attachmentId && x.PatrolCaseId == caseId && x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            // For PrimaryAdmin+, allow access to all attachments
            // For others, only allow if they are the creator or in the same assignment
            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                query = query.Where(att =>
                    _context.PatrolCases.Any(pc =>
                        pc.Id == caseId
                        && pc.Status != 0
                        && ((pc.Security != null && pc.Security.Email == userEmail)
                        || _context.PatrolAssignmentSecurities.Any(pas =>
                            pas.PatrolAssignmentId == pc.PatrolAssignmentId
                            && pas.Security != null
                            && pas.Security.Email == userEmail
                        ))
                    )
                );
            }

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Delete single attachment by ID
        /// </summary>
        public async Task<bool> DeleteAttachmentAsync(Guid caseId, Guid attachmentId)
        {
            var attachment = await GetAttachmentByIdAsync(caseId, attachmentId);
            if (attachment == null)
                return false;

            attachment.Status = 0;
            attachment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
