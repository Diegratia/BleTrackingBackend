using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.Shared.ExceptionHelper;
using Entities.Models;
using DataView;
using Repositories.Repository;

namespace BusinessLogic.Services.ServiceValidators
{
    public class PatrolValidationService : IPatrolValidationService
    {
        private readonly PatrolAssignmentRepository _repository;

        public PatrolValidationService(PatrolAssignmentRepository repository)
        {
            _repository = repository;
        }

        public async Task ValidateAssignmentCreateAsync(PatrolAssignmentCreateDto dto, Guid appId)
        {
            // 1. Existence Checks
            if (!dto.PatrolRouteId.HasValue)
                throw new BusinessException("PatrolRouteId is .");
            if (!dto.TimeGroupId.HasValue)
                throw new BusinessException("TimeGroupId is required.");

            if (!await _repository.PatrolRouteExistsAsync(dto.PatrolRouteId.Value))
                throw new NotFoundException($"PatrolRoute with id {dto.PatrolRouteId} not found");
            
            if (!await _repository.TimeGroupExistsAsync(dto.TimeGroupId.Value))
                throw new NotFoundException($"TimeGroup with id {dto.TimeGroupId} not found");
            
            if (!await _repository.CheckTimeGroupPatrolType(dto.TimeGroupId.Value))
                throw new BusinessException($"TimeGroup should be Patrol Type");

            // 2. Ownership Checks
            var invalidRouteId = await _repository.CheckInvalidRouteOwnershipAsync(dto.PatrolRouteId.Value, appId);
            if (invalidRouteId.Any())
                throw new UnauthorizedException($"PatrolRoute does not belong to this Application");

            var invalidTimeGroupId = await _repository.CheckInvalidTGOwnershipAsync(dto.TimeGroupId.Value, appId);
            if (invalidTimeGroupId.Any())
                throw new UnauthorizedException($"TimeGroup does not belong to this Application");

            // 3. Security Checks
            var securityIds = dto.SecurityIds?.Distinct().ToList() ?? new List<Guid>();
            if (!securityIds.Any())
                throw new BusinessException("At least one security is required.");

            var missingSecurityIds = await _repository.GetMissingSecurityIdsAsync(securityIds);
            if (missingSecurityIds.Any())
                throw new NotFoundException($"Security not found: {string.Join(", ", missingSecurityIds)}");

            var invalidSecurityIds = await _repository.GetInvalidSecurityIdsByApplicationAsync(securityIds, appId);
            if (invalidSecurityIds.Any())
                throw new UnauthorizedException($"Some SecurityIds do not belong to this Application: {string.Join(", ", invalidSecurityIds)}");

            // 4. Schedule Overlap Checks
            foreach (var securityId in securityIds)
            {
                var conflictingIds = await _repository.HasScheduleOverlapAsync(
                    securityId,
                    dto.TimeGroupId.Value,
                    dto.StartDate,
                    dto.EndDate,
                    excludeAssignmentId: null
                );

                if (conflictingIds.Any())
                {
                    throw new BusinessException(
                        $"Security with ID '{securityId}' already has conflicting patrol assignments " +
                        $"(IDs: {string.Join(", ", conflictingIds)}). Please adjust the schedule.");
                }
            }
        }

        public async Task ValidateAssignmentUpdateAsync(Guid id, PatrolAssignmentUpdateDto dto, Guid appId, PatrolAssignment currentAssignment)
        {
            // 1. Session Check
            await EnsureNoActiveSessionsAsync(id);

            // 2. Data Checks
            if (dto.PatrolRouteId.HasValue)
            {
                if (!await _repository.PatrolRouteExistsAsync(dto.PatrolRouteId.Value))
                    throw new NotFoundException($"PatrolRoute not found");
                
                var invalidRouteId = await _repository.CheckInvalidRouteOwnershipAsync(dto.PatrolRouteId.Value, appId);
                if (invalidRouteId.Any())
                    throw new UnauthorizedException($"PatrolRoute does not belong to this Application");
            }

            if (dto.TimeGroupId.HasValue)
            {
                if (!await _repository.TimeGroupExistsAsync(dto.TimeGroupId.Value))
                    throw new NotFoundException($"TimeGroup with id {dto.TimeGroupId} not found");

                var invalidTimeGroupId = await _repository.CheckInvalidTGOwnershipAsync(dto.TimeGroupId.Value, appId);
                if (invalidTimeGroupId.Any())
                    throw new UnauthorizedException($"TimeGroup does not belong to this Application");
            }

            // 3. Security Checks
            var newSecurityIds = dto.SecurityIds?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<Guid>();

            if (newSecurityIds.Any())
            {
                var missing = await _repository.GetMissingSecurityIdsAsync(newSecurityIds);
                if (missing.Any())
                    throw new NotFoundException($"Security not found: {string.Join(", ", missing)}");

                var invalidSecurityIds = await _repository.GetInvalidSecurityIdsByApplicationAsync(newSecurityIds, appId);
                if (invalidSecurityIds.Any())
                    throw new UnauthorizedException($"Some SecurityIds do not belong to this Application: {string.Join(", ", invalidSecurityIds)}");
            }

            // 4. Schedule Overlap Checks
            if ((dto.TimeGroupId.HasValue || dto.StartDate.HasValue || dto.EndDate.HasValue) && newSecurityIds.Any())
            {
                var timeGroupId = dto.TimeGroupId ?? currentAssignment.TimeGroupId;
                var startDate = dto.StartDate ?? currentAssignment.StartDate;
                var endDate = dto.EndDate ?? currentAssignment.EndDate;

                foreach (var securityId in newSecurityIds)
                {
                    var conflictingIds = await _repository.HasScheduleOverlapAsync(
                        securityId,
                        timeGroupId!.Value,
                        startDate,
                        endDate,
                        excludeAssignmentId: id
                    );

                    if (conflictingIds.Any())
                    {
                        throw new BusinessException(
                            $"Security with ID '{securityId}' already has conflicting patrol assignments " +
                            $"(IDs: {string.Join(", ", conflictingIds)}). Please adjust the schedule.");
                    }
                }
            }
        }

        public async Task ValidateShiftReplacementAsync(
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
                    excludeAssignmentId: null
                );

                if (conflictingIds.Any())
                {
                    throw new BusinessException(
                        $"Substitute Security already has conflicting patrol assignments " +
                        $"(IDs: {string.Join(", ", conflictingIds)}) during the requested replacement period.");
                }
            }
        }

        public async Task EnsureNoActiveSessionsAsync(Guid assignmentId)
        {
            var hasActiveSessions = await _repository.HasActiveSessionsAsync(assignmentId);
            if (hasActiveSessions)
            {
                throw new BusinessException($"Cannot perform this action on Patrol Assignment {assignmentId} because there are active patrol sessions running. Please complete or abort them first.");
            }
        }
    }
}
