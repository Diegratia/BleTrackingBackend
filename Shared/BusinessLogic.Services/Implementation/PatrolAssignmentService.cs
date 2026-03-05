using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using DataView;
using BusinessLogic.Services.Extension;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolAssignmentService : BaseService, IPatrolAssignmentService
    {
        private readonly PatrolAssignmentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;


        public PatrolAssignmentService(
            PatrolAssignmentRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit
            ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
        }

        public async Task<PatrolAssignmentRead?> GetByIdAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");
            return patrolAssignment;
        }

        public async Task<IEnumerable<PatrolAssignmentRead>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return patrolAreas;
        }

        public async Task<PatrolAssignmentRead> CreateAsync(PatrolAssignmentCreateDto createDto)
        {

            if (!await _repository.PatrolRouteExistsAsync(createDto.PatrolRouteId!.Value))
                throw new NotFoundException($"PatrolRoute with id {createDto.PatrolRouteId} not found");
            if (!await _repository.TimeGroupExistsAsync(createDto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup with id {createDto.TimeGroupId} not found");
            if (!await _repository.CheckTimeGroupPatrolType(createDto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup should be Patrol Type");

            // Check for schedule overlap for each security
            if (createDto.TimeGroupId.HasValue && createDto.SecurityIds?.Any() == true)
            {
                foreach (var securityId in createDto.SecurityIds)
                {
                    var conflictingIds = await _repository.HasScheduleOverlapAsync(
                        securityId,
                        createDto.TimeGroupId.Value,
                        createDto.StartDate,
                        createDto.EndDate,
                        excludeAssignmentId: null  // New assignment, no exclude
                    );

                    if (conflictingIds.Any())
                    {
                        throw new BusinessException(
                            $"Security with ID '{securityId}' already has conflicting patrol assignments " +
                            $"(IDs: {string.Join(", ", conflictingIds)}). Please adjust the schedule.");
                    }
                }
            }

            var invalidSecurityIds =
            await _repository.GetInvalidSecurityIdsByApplicationAsync(createDto.SecurityIds, AppId);
            if (invalidSecurityIds.Any())
            {
                throw new UnauthorizedException(
                    $"Some SecurityIds do not belong to this Application: {string.Join(", ", invalidSecurityIds)}"
                );
            }
            var invalidRouteId =
            await _repository.CheckInvalidRouteOwnershipAsync(createDto.PatrolRouteId.Value, AppId);
            if (invalidRouteId.Any())
            {
                throw new UnauthorizedException(
                    $"Some PatrolRouteId do not belong to this Application: {string.Join(", ", invalidRouteId)}"
                );
            }
            var invalidTimeGroupIdd =
            await _repository.CheckInvalidTGOwnershipAsync(createDto.TimeGroupId.Value, AppId);
            if (invalidTimeGroupIdd.Any())
            {
                throw new UnauthorizedException(
                    $"Some TimeGroupId do not belong to this Application: {string.Join(", ", invalidTimeGroupIdd)}"
                );
            }


            var missingSecurityIds = await _repository
                .GetMissingSecurityIdsAsync(createDto.SecurityIds ?? new List<Guid>());

            if (missingSecurityIds.Count > 0)
            {
                throw new NotFoundException(
                    $"Security not found: {string.Join(", ", missingSecurityIds)}"
                );
            }


            var patrolAssignment = _mapper.Map<PatrolAssignment>(createDto);
            
            // Validate and assign Security Heads
            if (createDto.SecurityHead1Id.HasValue && createDto.SecurityHead2Id.HasValue)
            {
                // Optional: Validate if the selected heads are valid for the selected securities
                // var isValidHeads = await _repository.ValidateSecurityHeadsAsync(createDto.SecurityIds, createDto.SecurityHead1Id.Value, createDto.SecurityHead2Id.Value);
                // if (!isValidHeads) throw new BusinessException("Selected Security Heads are not valid for the selected Security personnel.");
                
                patrolAssignment.SecurityHead1Id = createDto.SecurityHead1Id.Value;
                patrolAssignment.SecurityHead2Id = createDto.SecurityHead2Id.Value;
            }

            if (!createDto.ApprovalType.HasValue)
                patrolAssignment.ApprovalType = PatrolApprovalType.ByThreatLevel;
            SetCreateAudit(patrolAssignment);

            patrolAssignment.PatrolAssignmentSecurities = new List<PatrolAssignmentSecurity>();

            foreach (var securityId in createDto.SecurityIds!.Distinct())
            {
                patrolAssignment.PatrolAssignmentSecurities.Add(
                    new PatrolAssignmentSecurity
                    {
                        SecurityId = securityId,
                        ApplicationId = AppId,
                        CreatedBy = UsernameFormToken,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedBy = UsernameFormToken,
                        UpdatedAt = DateTime.UtcNow
                    });
            }

            await _repository.AddAsync(patrolAssignment);
             _audit.Created(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Created patrolAssignment",
                new { patrolAssignment.Name }
            );
            var result = await _repository.GetByIdAsync(patrolAssignment.Id);
            return result!;
        }

        public async Task<PatrolAssignmentRead> UpdateAsync(Guid id, PatrolAssignmentUpdateDto dto)
        {
            var assignment = await _repository.GetByIdWithTrackingAsync(id)
                ?? throw new NotFoundException($"PatrolAssignment {id} not found");

            // ================= VALIDASI =================
            
            // PREVENT UPDATE IF THERE ARE ACTIVE SESSIONS
            var hasActiveSessions = await _repository.HasActiveSessionsAsync(id);
            if (hasActiveSessions)
            {
                throw new BusinessException($"Cannot update Patrol Assignment {id} because there are active patrol sessions running. Please complete or abort them first.");
            }

            if (dto.PatrolRouteId.HasValue &&
                !await _repository.PatrolRouteExistsAsync(dto.PatrolRouteId.Value))
                throw new NotFoundException($"PatrolRoute not found");
            if (dto.TimeGroupId.HasValue &&
                !await _repository.TimeGroupExistsAsync(dto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup with id {dto.TimeGroupId} not found");

            // Check for schedule overlap when updating schedule or securities
            var newSecurityIds = dto.SecurityIds?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if ((dto.TimeGroupId.HasValue || dto.StartDate.HasValue || dto.EndDate.HasValue) &&
                newSecurityIds.Any())
            {
                var timeGroupId = dto.TimeGroupId ?? assignment.TimeGroupId;
                var startDate = dto.StartDate ?? assignment.StartDate;
                var endDate = dto.EndDate ?? assignment.EndDate;

                foreach (var securityId in newSecurityIds)
                {
                    var conflictingIds = await _repository.HasScheduleOverlapAsync(
                        securityId,
                        timeGroupId!.Value,
                        startDate,
                        endDate,
                        excludeAssignmentId: id  // Exclude current assignment
                    );

                    if (conflictingIds.Any())
                    {
                        throw new BusinessException(
                            $"Security with ID '{securityId}' already has conflicting patrol assignments " +
                            $"(IDs: {string.Join(", ", conflictingIds)}). Please adjust the schedule.");
                    }
                }
            }

            if (dto.PatrolRouteId.HasValue)
            {
                var invalidRouteId =
                    await _repository.CheckInvalidRouteOwnershipAsync(dto.PatrolRouteId.Value, AppId);
                if (invalidRouteId.Any())
                {
                    throw new UnauthorizedException(
                        $"Some PatrolRouteId do not belong to this Application: {string.Join(", ", invalidRouteId)}"
                    );
                }
            }

            if (dto.TimeGroupId.HasValue)
            {
                var invalidTimeGroupId =
                    await _repository.CheckInvalidTGOwnershipAsync(dto.TimeGroupId.Value, AppId);
                if (invalidTimeGroupId.Any())
                {
                    throw new UnauthorizedException(
                        $"Some TimeGroupId do not belong to this Application: {string.Join(", ", invalidTimeGroupId)}"
                    );
                }
            }

            var missing = await _repository.GetMissingSecurityIdsAsync(newSecurityIds);
                await _repository.GetMissingSecurityIdsAsync(newSecurityIds);

            if (missing.Count > 0)
                throw new NotFoundException(
                    $"Security not found: {string.Join(", ", missing)}");

            var invalidSecurityIds =
                await _repository.GetInvalidSecurityIdsByApplicationAsync(newSecurityIds, AppId);
            if (invalidSecurityIds.Any())
            {
                throw new UnauthorizedException(
                    $"Some SecurityIds do not belong to this Application: {string.Join(", ", invalidSecurityIds)}"
                );
            }


            // 1. Replace all child (SQL-style)
            await _repository.RemoveAllPatrolAssignmentSecurities(id);

            foreach (var secId in dto.SecurityIds!.Distinct())
            {
                await _repository.AddPatrolAssignmentSecurityAsync(
                    new PatrolAssignmentSecurity
                    {
                        PatrolAssignmentId = assignment.Id,
                        SecurityId = secId!.Value,
                        ApplicationId = assignment.ApplicationId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken
                    });
            }

            // 2. Update parent scalar only
            _mapper.Map(dto, assignment);
            if (dto.SecurityHead1Id.HasValue && dto.SecurityHead2Id.HasValue)
            {
                 assignment.SecurityHead1Id = dto.SecurityHead1Id.Value;
                 assignment.SecurityHead2Id = dto.SecurityHead2Id.Value;
            }

            if (!dto.ApprovalType.HasValue)
                assignment.ApprovalType = PatrolApprovalType.WithoutApproval;
            SetUpdateAudit(assignment);

            await _repository.UpdateAsync(assignment);

            var result = await _repository.GetByIdAsync(id);
             _audit.Updated("Patrol Assignment", id, "Updated", new { result!.Name });

            return result!;
        }



        public async Task DeleteAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdWithTrackingAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");

            // PREVENT DELETE IF THERE ARE ACTIVE SESSIONS
            var hasActiveSessions = await _repository.HasActiveSessionsAsync(id);
            if (hasActiveSessions)
            {
                throw new BusinessException($"Cannot delete Patrol Assignment {id} because there are active patrol sessions running. Please complete or abort them first.");
            }

            await _repository.ExecuteInTransactionAsync(async () =>
            {
                SetDeleteAudit(patrolAssignment);
                patrolAssignment.Status = 0;
                await _repository.RemoveAssignmentSecurities(id);
                await _repository.DeleteAsync(id);
            });
             _audit.Deleted(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Deleted patrolAssignment",
                new { patrolAssignment.Name }
            );
        }


        public async Task<IEnumerable<PatrolAssignmentLookUpRead>> GetAllLookUpAsync()
        {
            var patrolareas = await _repository.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<PatrolAssignmentLookUpRead>>(patrolareas);
        }

        // projection filter
            public async Task<object> FilterProjectedAsync(
            DataTablesProjectedRequest request,
            PatrolAssignmentFilter filter
        )
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = data
            };
        }

        public async Task<PatrolShiftReplacementRead> AddShiftReplacementAsync(PatrolShiftReplacementCreateDto createDto)
        {
            var assignment = await _repository.GetByIdWithTrackingAsync(createDto.PatrolAssignmentId);
            if (assignment == null)
                throw new NotFoundException($"PatrolAssignment {createDto.PatrolAssignmentId} not found");

            await ValidateShiftReplacementAsync(
                assignment, 
                createDto.OriginalSecurityId, 
                createDto.SubstituteSecurityId, 
                createDto.ReplacementStartDate, 
                createDto.ReplacementEndDate
            );

            var entity = new PatrolShiftReplacement
            {
                PatrolAssignmentId = createDto.PatrolAssignmentId,
                OriginalSecurityId = createDto.OriginalSecurityId,
                SubstituteSecurityId = createDto.SubstituteSecurityId,
                ReplacementStartDate = createDto.ReplacementStartDate,
                ReplacementEndDate = createDto.ReplacementEndDate,
                Reason = createDto.Reason,
                ApplicationId = AppId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = UsernameFormToken,
                UpdatedBy = UsernameFormToken
            };

            await _repository.AddShiftReplacementAsync(entity);
            
            _audit.Created(
                "Shift Replacement",
                entity.Id,
                "Created shift replacement",
                new { assignment.Name, OriginalSecurityId = createDto.OriginalSecurityId, SubstituteSecurityId = createDto.SubstituteSecurityId }
            );

            // Fetch to return DTO properly
            var saved = await _repository.GetShiftReplacementByIdAsync(entity.Id);
            return new PatrolShiftReplacementRead
            {
                Id = saved!.Id,
                PatrolAssignmentId = saved.PatrolAssignmentId,
                OriginalSecurity = saved.OriginalSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.OriginalSecurity.Id,
                    Name = saved.OriginalSecurity.Name,
                    CardNumber = saved.OriginalSecurity.CardNumber,
                    IdentityId = saved.OriginalSecurity.IdentityId,
                },
                SubstituteSecurity = saved.SubstituteSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.SubstituteSecurity.Id,
                    Name = saved.SubstituteSecurity.Name,
                    CardNumber = saved.SubstituteSecurity.CardNumber,
                    IdentityId = saved.SubstituteSecurity.IdentityId,
                },
                ReplacementStartDate = saved.ReplacementStartDate,
                ReplacementEndDate = saved.ReplacementEndDate,
                Reason = saved.Reason
            };
        }

        public async Task RemoveShiftReplacementAsync(Guid id)
        {
            var entity = await _repository.GetShiftReplacementByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolShiftReplacement {id} not found");

            // Prevent deletion if replacement date has started
            if (DateOnly.FromDateTime(DateTime.UtcNow) >= entity.ReplacementStartDate)
            {
                throw new BusinessException($"Cannot delete shift replacement because the replacement date has already started or passed.");
            }

            await _repository.RemoveShiftReplacementAsync(entity);

            _audit.Deleted(
                "Shift Replacement",
                id,
                "Deleted shift replacement",
                new { entity.PatrolAssignmentId }
            );
        }

        public async Task<PatrolShiftReplacementRead> UpdateShiftReplacementAsync(Guid id, PatrolShiftReplacementUpdateDto dto)
        {
            var entity = await _repository.GetShiftReplacementByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolShiftReplacement {id} not found");
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            // Prevent update if replacement date has started
            if (today >= entity.ReplacementStartDate)
            {
                throw new BusinessException($"Cannot update shift replacement because the replacement date has already started or passed.");
            }

            var assignment = await _repository.GetByIdWithTrackingAsync(entity.PatrolAssignmentId);
            
            var originalSecurityId = dto.OriginalSecurityId ?? entity.OriginalSecurityId;
            var substituteSecurityId = dto.SubstituteSecurityId ?? entity.SubstituteSecurityId;
            var startDate = dto.ReplacementStartDate ?? entity.ReplacementStartDate;
            var endDate = dto.ReplacementEndDate ?? entity.ReplacementEndDate;

            await ValidateShiftReplacementAsync(
                assignment!, 
                originalSecurityId, 
                substituteSecurityId, 
                startDate, 
                endDate, 
                id
            );

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = UsernameFormToken;

            await _repository.UpdateShiftReplacementAsync(entity);

            _audit.Updated(
                "Shift Replacement",
                id,
                "Updated shift replacement",
                new { entity.PatrolAssignmentId }
            );

            // Fetch to return DTO properly
            var saved = await _repository.GetShiftReplacementByIdAsync(entity.Id);
            return new PatrolShiftReplacementRead
            {
                Id = saved!.Id,
                PatrolAssignmentId = saved.PatrolAssignmentId,
                OriginalSecurity = saved.OriginalSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.OriginalSecurity.Id,
                    Name = saved.OriginalSecurity.Name,
                    CardNumber = saved.OriginalSecurity.CardNumber,
                    IdentityId = saved.OriginalSecurity.IdentityId,
                },
                SubstituteSecurity = saved.SubstituteSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.SubstituteSecurity.Id,
                    Name = saved.SubstituteSecurity.Name,
                    CardNumber = saved.SubstituteSecurity.CardNumber,
                    IdentityId = saved.SubstituteSecurity.IdentityId,
                },
                ReplacementStartDate = saved.ReplacementStartDate,
                ReplacementEndDate = saved.ReplacementEndDate,
                Reason = saved.Reason
            };
        }

        private async Task ValidateShiftReplacementAsync(
            PatrolAssignment assignment, 
            Guid originalSecurityId, 
            Guid substituteSecurityId, 
            DateOnly startDate, 
            DateOnly endDate,
            Guid? excludeReplacementId = null)
        {
            // 1. Ensure OriginalSecurity is assigned to this assignment
            var isAssigned = await _repository.IsSecurityAssignedToAssignmentAsync(assignment.Id, originalSecurityId);
            if (!isAssigned)
                throw new BusinessException("Original Security is not assigned to this patrol assignment.");

            // 2. Validate dates are within assignment range
            if ((assignment.StartDate.HasValue && startDate < assignment.StartDate.Value) || 
                (assignment.EndDate.HasValue && endDate > assignment.EndDate.Value))
            {
                var startDateStr = assignment.StartDate?.ToString("yyyy-MM-dd") ?? "N/A";
                var endDateStr = assignment.EndDate.HasValue ? assignment.EndDate.Value.ToString("yyyy-MM-dd") : "Open Ended";
                throw new BusinessException($"Replacement dates must be within assignment date range ({startDateStr} to {endDateStr}).");
            }

            // 3. Check for schedule overlap for Substitute Security
            if (assignment.TimeGroupId.HasValue)
            {
                var conflictingIds = await _repository.HasScheduleOverlapAsync(
                    substituteSecurityId,
                    assignment.TimeGroupId.Value,
                    startDate,
                    endDate,
                    excludeAssignmentId: null // We check against OTHER assignments
                );

                if (conflictingIds.Any())
                {
                    throw new BusinessException(
                        $"Substitute Security already has conflicting patrol assignments " +
                        $"(IDs: {string.Join(", ", conflictingIds)}) during the requested replacement period.");
                }
            }
        }

    }
}
