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
using Shared.Contracts;
using System.Security.Claims;
using Repositories.Extensions;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class PatrolAssignmentRepository : BaseRepository
    {
        public PatrolAssignmentRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        public async Task<IEnumerable<PatrolAssignmentRead>> GetAllAsync()
        {
            return await GetAllProjectedQueryable(BaseEntityQuery()).ToListAsync();
        }

        public async Task<PatrolAssignment> AddAsync(PatrolAssignment patrolassignment)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                patrolassignment.ApplicationId = applicationId.Value;
            }
            else if (patrolassignment.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(patrolassignment.ApplicationId);
            ValidateApplicationIdForEntity(patrolassignment, applicationId, isSystemAdmin);

            _context.PatrolAssignments.Add(patrolassignment);
            await _context.SaveChangesAsync();
            return patrolassignment;
        }

        public async Task UpdateAsync(PatrolAssignment patrolassignment)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(patrolassignment.ApplicationId);
            ValidateApplicationIdForEntity(patrolassignment, applicationId, isSystemAdmin);
            await _context.SaveChangesAsync();
        }




        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.PatrolAssignments
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var patrolassignment = await query.FirstOrDefaultAsync();

            if (patrolassignment == null)
                return;

            await _context.SaveChangesAsync();
        }

        // Di PatrolAssignmentRepository
        public async Task<PatrolAssignmentRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(x => x.Id == id);
            return await GetAllProjectedQueryable(query).FirstOrDefaultAsync();
        }

        // Di Repository
        public async Task<PatrolAssignment?> GetByIdWithTrackingAsync(Guid id)
        {
            return await BaseEntityQuery() 
                .FirstOrDefaultAsync(a => a.Id == id); 
        }


        // public IQueryable<PatrolAssignment> GetAllQueryable()
        // {

        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
        //     var query = _context.PatrolAssignments
        //     .Include(d => d.PatrolRoute)
        //     .Include(d => d.PatrolAssignmentSecurities)
        //         .ThenInclude(pas => pas.Security)
        //     .Where(d => d.Status != 0);
        //     return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        // }
        public IQueryable<PatrolAssignment> GetAllQueryable()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolAssignments
                .Include(d => d.PatrolRoute)
                .Include(d => d.TimeGroup)
                .Include(d => d.PatrolAssignmentSecurities)
                    .ThenInclude(pas => pas.Security)
                .Where(d => d.Status != 0);

            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                var today = DateTime.UtcNow.Date;
                query = query.Where(pa =>
                    pa.PatrolAssignmentSecurities.Any(pas =>
                        pas.Security != null &&
                        pas.Security.Email == userEmail
                    ) ||
                    pa.PatrolShiftReplacements.Any(psr =>
                        psr.SubstituteSecurity != null &&
                        psr.SubstituteSecurity.Email == userEmail &&
                        psr.ReplacementStartDate.Date <= today &&
                        psr.ReplacementEndDate.Date >= today &&
                        psr.Status != 0
                    )
                );
            }

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<List<PatrolAssignmentLookUpRead>> GetAllLookUpAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = BaseEntityQuery().AsNoTracking();

            var projected = query.Select(ca => new PatrolAssignmentLookUpRead
            {
                Id = ca.Id,
                Name = ca.Name,
                Description = ca.Description,
            });
            return await projected.ToListAsync();
        }

        //Projection Query
        
        private IQueryable<PatrolAssignment> BaseEntityQuery()
        {
            var userEmail = GetUserEmail();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var isSuperAdmin = IsSuperAdmin();
            var isPrimaryAdmin = IsPrimaryAdmin();

            var query = _context.PatrolAssignments
                .Where(x => x.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            if (!isSystemAdmin && !isSuperAdmin && !isPrimaryAdmin)
            {
                var today = DateTime.UtcNow.Date;
                query = query.Where(pa =>
                    pa.PatrolAssignmentSecurities.Any(pas =>
                        pas.Security.Email == userEmail
                    ) ||
                    pa.PatrolShiftReplacements.Any(psr =>
                        psr.SubstituteSecurity != null &&
                        psr.SubstituteSecurity.Email == userEmail &&
                        psr.ReplacementStartDate.Date <= today &&
                        psr.ReplacementEndDate.Date >= today &&
                        psr.Status != 0
                    )
                );
            }

            return query;
        }




    public async Task<(List<PatrolAssignmentRead> Data, int Total, int Filtered)>
            FilterAsync(PatrolAssignmentFilter filter)
        {
            var query = BaseEntityQuery();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(x =>
                    x.Name!.ToLower().Contains(search) ||
                    x.Description!.ToLower().Contains(search)
                );
            }

            if (filter.PatrolRouteId.HasValue)
                query = query.Where(x => x.PatrolRouteId == filter.PatrolRouteId);

            if(filter.CycleType.HasValue)
                query = query.Where(x => x.CycleType == filter.CycleType);
            
            if(filter.StartType.HasValue)
                query = query.Where(x => x.StartType == filter.StartType);

            if(filter.DurationType.HasValue)
                query = query.Where(x => x.DurationType == filter.DurationType);

            if(filter.ApprovalType.HasValue)
                query = query.Where(x => x.ApprovalType == filter.ApprovalType);

            if (filter.TimeGroupId.HasValue)
                query = query.Where(x => x.TimeGroupId == filter.TimeGroupId);

            var filtered = await query.CountAsync();

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await GetAllProjectedQueryable(query).ToListAsync();
            return (data, total, filtered);
        }

            private IQueryable<PatrolAssignmentRead> GetAllProjectedQueryable(
                IQueryable<PatrolAssignment> query
            )
        {
            return query.AsNoTracking().Select(pa => new PatrolAssignmentRead
            {
                Id = pa.Id,
                Name = pa.Name,
                Description = pa.Description,
                PatrolRouteId = pa.PatrolRouteId,
                TimeGroupId = pa.TimeGroupId,
                ApprovalType = pa.ApprovalType,
                DurationType = pa.DurationType,
                StartType = pa.StartType,
                CycleType = pa.CycleType,
                StartDate = pa.StartDate,
                EndDate = pa.EndDate,
                ApplicationId = pa.ApplicationId,
                PatrolRoute = pa.PatrolRoute == null ? null : new PatrolRouteLookUpRead
                {
                    Id = pa.PatrolRoute.Id,
                    Name = pa.PatrolRoute.Name,
                    Description = pa.PatrolRoute.Description,
                    StartAreaName = pa.PatrolRoute.PatrolRouteAreas
                        .OrderBy(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault(),
                    EndAreaName = pa.PatrolRoute.PatrolRouteAreas
                        .OrderByDescending(x => x.OrderIndex)
                        .Select(x => x.PatrolArea.Name)
                        .FirstOrDefault()
                },

                TimeGroup = pa.TimeGroup == null ? null : new AssignmentTimeGroupRead
                {
                    Id = pa.TimeGroup.Id,
                    Name = pa.TimeGroup.Name,
                    ScheduleType = pa.TimeGroup.ScheduleType.ToString(),
                    TimeBlocks = pa.TimeGroup.TimeBlocks
                        .Select(x => new TimeBlockRead
                        {
                            Id = x.Id,
                            DayOfWeek = x.DayOfWeek,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime
                        })
                        .ToList()
                },
                Securities = pa.PatrolAssignmentSecurities
                    .Select(x => new  SecurityListRead
                    {
                        Id = x.Security.Id,
                        Name = x.Security.Name,
                        CardNumber = x.Security.CardNumber,
                        IdentityId = x.Security.IdentityId,
                        OrganizationName = x.Security.Organization.Name,
                        DepartmentName = x.Security.Department.Name,
                        DistrictName = x.Security.District.Name
                    })
                    .ToList(),
                ShiftReplacements = pa.PatrolShiftReplacements
                    .Select(r => new PatrolShiftReplacementRead
                    {
                        Id = r.Id,
                        PatrolAssignmentId = r.PatrolAssignmentId,
                        OriginalSecurity = r.OriginalSecurity == null ? null : new SecurityListRead
                        {
                            Id = r.OriginalSecurity.Id,
                            Name = r.OriginalSecurity.Name,
                            CardNumber = r.OriginalSecurity.CardNumber,
                            IdentityId = r.OriginalSecurity.IdentityId,
                            OrganizationName = r.OriginalSecurity.Organization.Name,
                            DepartmentName = r.OriginalSecurity.Department.Name,
                            DistrictName = r.OriginalSecurity.District.Name
                        },
                        SubstituteSecurity = r.SubstituteSecurity == null ? null : new SecurityListRead
                        {
                            Id = r.SubstituteSecurity.Id,
                            Name = r.SubstituteSecurity.Name,
                            CardNumber = r.SubstituteSecurity.CardNumber,
                            IdentityId = r.SubstituteSecurity.IdentityId,
                            OrganizationName = r.OriginalSecurity.Organization.Name,
                            DepartmentName = r.OriginalSecurity.Department.Name,
                            DistrictName = r.OriginalSecurity.District.Name,
                        },
                        ReplacementStartDate = r.ReplacementStartDate,
                        ReplacementEndDate = r.ReplacementEndDate,
                        Reason = r.Reason
                    }).ToList(),
                SecurityHead1 = pa.SecurityHead1 == null ? null : new SecurityListRead
                {
                    Id = pa.SecurityHead1.Id,
                    Name = pa.SecurityHead1.Name,
                    CardNumber = pa.SecurityHead1.CardNumber,
                    IdentityId = pa.SecurityHead1.IdentityId
                },
                SecurityHead2 = pa.SecurityHead2 == null ? null : new SecurityListRead
                {
                    Id = pa.SecurityHead2.Id,
                    Name = pa.SecurityHead2.Name,
                    CardNumber = pa.SecurityHead2.CardNumber,
                    IdentityId = pa.SecurityHead2.IdentityId
                }
            });
        }

        public async Task<PatrolShiftReplacement> AddShiftReplacementAsync(PatrolShiftReplacement entity)
        {
            await _context.PatrolShiftReplacements.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PatrolShiftReplacement?> GetShiftReplacementByIdAsync(Guid id)
        {
            return await _context.PatrolShiftReplacements
                .Include(r => r.OriginalSecurity)
                .Include(r => r.SubstituteSecurity)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task RemoveShiftReplacementAsync(PatrolShiftReplacement entity)
        {
            _context.PatrolShiftReplacements.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateShiftReplacementAsync(PatrolShiftReplacement entity)
        {
            _context.PatrolShiftReplacements.Update(entity);
            await _context.SaveChangesAsync();
        }



        //Helpers

        public async Task RemoveAllPatrolAssignmentSecurities(Guid assignmentId)
        {
            await _context.PatrolAssignmentSecurities
                .Where(x => x.PatrolAssignmentId == assignmentId)
                .ExecuteDeleteAsync();
        }

        public async Task AddPatrolAssignmentSecurityAsync(PatrolAssignmentSecurity entity)
        {
            await _context.PatrolAssignmentSecurities.AddAsync(entity);
        }

        // public async Task UpdateScalarAsync(PatrolAssignment assignment)
        // {
        //     _context.PatrolAssignments.Update(assignment);
        //     await _context.SaveChangesAsync();
        // }


        public void Attach(PatrolAssignment entity)
        {
            _context.Attach(entity);
        }


        public async Task<IReadOnlyCollection<Guid>> GetMissingSecurityIdsAsync(
    IEnumerable<Guid> ids
    )
        {
            var idList = ids.Distinct().ToList();
            if (!idList.Any())
                return Array.Empty<Guid>();

            var existingIds = await _context.MstSecurities
                .Where(x => idList.Contains(x.Id) && x.Status != 0)
                .Select(x => x.Id)
                .ToListAsync();

            return idList.Except(existingIds).ToList();
        }

        public async Task<bool> PatrolRouteExistsAsync(Guid patrolRouteId)
        {
            return await _context.PatrolRoutes
                .AnyAsync(f => f.Id == patrolRouteId && f.Status != 0);
        }
        public async Task<bool> TimeGroupExistsAsync(Guid timeGroupId)
        {
            return await _context.TimeGroups
                .AnyAsync(f => f.Id == timeGroupId && f.Status != 0);
        }
        public async Task<bool> CheckTimeGroupPatrolType(Guid timeGroupId)
        {
            return await _context.TimeGroups
                .AnyAsync(f => f.Id == timeGroupId && f.Status != 0 && f.ScheduleType == ScheduleType.Patrol);
        }

        public async Task<bool> ValidateSecurityHeadsAsync(IEnumerable<Guid> securityIds, Guid head1Id, Guid head2Id)
        {
            var validHeads = await _context.MstSecurities
                .Where(x => securityIds.Contains(x.Id) && x.Status != 0)
                .SelectMany(x => new[] { x.SecurityHead1Id, x.SecurityHead2Id })
                .Where(h => h.HasValue)
                .Select(h => h!.Value)
                .Distinct()
                .ToListAsync();

            return validHeads.Contains(head1Id) && validHeads.Contains(head2Id);
        }

        public async Task RemoveAssignmentSecurities(Guid patrolAssignmentId)
        {
            var rows = _context.PatrolAssignmentSecurities
                .Where(x => x.PatrolAssignmentId == patrolAssignmentId);

            _context.PatrolAssignmentSecurities.RemoveRange(rows);
            await _context.SaveChangesAsync();
        }


        public async Task<IReadOnlyCollection<Guid>> GetInvalidSecurityIdsByApplicationAsync(
    IEnumerable<Guid> securityIds,
    Guid applicationId
        )
        {
            var ids = securityIds.Distinct().ToList();
            if (!ids.Any())
                return Array.Empty<Guid>();

            var validIds = await _context.MstSecurities
                .Where(s =>
                    ids.Contains(s.Id) &&
                    s.ApplicationId == applicationId &&
                    s.Status != 0
                )
                .Select(s => s.Id)
                .ToListAsync();

            return ids.Except(validIds).ToList();
        }
        
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidRouteOwnershipAsync(
            Guid routeId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<PatrolRoute>(
                new[] { routeId },
                applicationId
            );
        }
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidTGOwnershipAsync(
            Guid timeGroupId,
            Guid applicationId
        )
        {
            return await CheckInvalidOwnershipIdsAsync<TimeGroup>(
                new[] { timeGroupId },
                applicationId
            );
        }

        public async Task<bool> IsSecurityAssignedToAssignmentAsync(Guid assignmentId, Guid securityId)
        {
            return await _context.PatrolAssignmentSecurities
                .AnyAsync(pas => pas.PatrolAssignmentId == assignmentId && pas.SecurityId == securityId && pas.Status != 0);
        }

        public async Task<bool> HasActiveSessionsAsync(Guid assignmentId)
        {
            return await _context.PatrolSessions
                .AnyAsync(s => s.PatrolAssignmentId == assignmentId && s.EndedAt == null && s.Status != 0);
        }

        /// <summary>
        /// Check if a security has conflicting patrol assignments (overlapping schedule)
        /// </summary>
        /// <param name="securityId">Security to check</param>
        /// <param name="timeGroupId">TimeGroup of new assignment</param>
        /// <param name="startDate">Start date of new assignment</param>
        /// <param name="endDate">End date of new assignment</param>
        /// <param name="excludeAssignmentId">Exclude current assignment (for update)</param>
        /// <returns>List of conflicting assignment IDs</returns>
        public async Task<List<Guid>> HasScheduleOverlapAsync(
            Guid securityId,
            Guid timeGroupId,
            DateTime? startDate,
            DateTime? endDate,
            Guid? excludeAssignmentId = null)
        {
            // Get TimeBlocks for the new assignment's TimeGroup
            var newTimeBlocks = await _context.TimeBlocks
                .Where(tb => tb.TimeGroupId == timeGroupId && tb.Status != 0)
                .ToListAsync();

            if (!newTimeBlocks.Any())
                return new List<Guid>();

            // Query existing assignments for the same security
            var query = _context.PatrolAssignmentSecurities
                .Where(pas => pas.SecurityId == securityId && pas.Status != 0)
                .Include(pas => pas.PatrolAssignment)
                    .ThenInclude(pa => pa.TimeGroup)
                        .ThenInclude(tg => tg.TimeBlocks)
                .Where(pas => pas.PatrolAssignment.Status != 0)
                .Where(pas => pas.PatrolAssignment.TimeGroupId != null);

            // Exclude current assignment (for update scenario)
            if (excludeAssignmentId.HasValue)
                query = query.Where(pas => pas.PatrolAssignmentId != excludeAssignmentId.Value);

            // Filter by date range overlap
            if (startDate.HasValue)
                query = query.Where(pas => pas.PatrolAssignment.EndDate == null ||
                                          pas.PatrolAssignment.EndDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(pas => pas.PatrolAssignment.StartDate == null ||
                                          pas.PatrolAssignment.StartDate <= endDate.Value);

            var existingAssignments = await query.ToListAsync();
            var conflictingIds = new List<Guid>();

            foreach (var existing in existingAssignments)
            {
                if (existing.PatrolAssignment?.TimeGroup?.TimeBlocks == null)
                    continue;

                // Check if any TimeBlock overlaps
                foreach (var newBlock in newTimeBlocks)
                {
                    foreach (var existingBlock in existing.PatrolAssignment.TimeGroup.TimeBlocks)
                    {
                        // Skip if existing block is inactive
                        if (existingBlock.Status == 0)
                            continue;

                        // Same day of week?
                        if (newBlock.DayOfWeek != existingBlock.DayOfWeek)
                            continue;

                        // Check time overlap
                        if (HasTimeOverlap(
                            newBlock.StartTime, newBlock.EndTime,
                            existingBlock.StartTime, existingBlock.EndTime))
                        {
                            conflictingIds.Add(existing.PatrolAssignmentId);
                            break;
                        }
                    }

                    if (conflictingIds.Contains(existing.PatrolAssignmentId))
                        break;
                }
            }

            return conflictingIds.Distinct().ToList();
        }

        /// <summary>
        /// Check if two time ranges overlap
        /// </summary>
        private static bool HasTimeOverlap(TimeSpan? start1, TimeSpan? end1,
                                             TimeSpan? start2, TimeSpan? end2)
        {
            if (!start1.HasValue || !end1.HasValue || !start2.HasValue || !end2.HasValue)
                return false;

            // Two ranges overlap if: start1 < end2 AND end1 > start2
            return start1.Value < end2.Value && end1.Value > start2.Value;
        }
    }
}
