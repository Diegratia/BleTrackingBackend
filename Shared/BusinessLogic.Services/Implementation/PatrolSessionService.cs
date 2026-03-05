using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Background;
using BusinessLogic.Services.Extension.FileStorageService;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolSessionService : BaseService, IPatrolSessionService
    {
        private readonly PatrolSessionRepository _repo;
        private readonly IPatrolCaseService _caseService;
        private readonly IAuditEmitter _audit;
        private readonly IMqttPubQueue _mqttQueue;

        public PatrolSessionService(
            PatrolSessionRepository repo,
            IPatrolCaseService caseService,
            IAuditEmitter audit,
            IMqttPubQueue mqttQueue,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
            _caseService = caseService;
            _audit = audit;
            _mqttQueue = mqttQueue;
        }
        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            PatrolSessionFilter filter
        )
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn;
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;
            filter.TimeRange = request.TimeRange;

            var (data, total, filtered) = await _repo.FilterAsync(filter);
            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        public async Task<PatrolSessionRead?> GetByIdAsync(Guid id)
        {
            var patrolSession = await _repo.GetByIdAsync(id);
            if (patrolSession == null)
                throw new NotFoundException($"patrolSession with id {id} not found");
            return patrolSession;
        }

        public async Task<IEnumerable<PatrolSessionRead>> GetAllAsync()
        {
            var patrolSessions = await _repo.GetAllAsync();
            return patrolSessions;
        }

        public async Task<IEnumerable<PatrolSessionLookUpRead>> GetAllLookUpAsync()
        {
            var patrolareas = await _repo.GetAllLookUpAsync();
            return patrolareas;
        }

        public async Task<PatrolSessionRead> CreateAsync(PatrolSessionStartDto dto)
        {
            var assignment = await _repo.GetAssignmentByIdAsync(dto.PatrolAssignmentId!.Value);

            if (assignment == null)
                throw new NotFoundException("PatrolAssignment not found");

            var security = await _repo.GetSecurityByEmail();
            if (security == null)
                throw new UnauthorizedAccessException("Security not found");

            var nowUtc = DateTime.UtcNow;

            // 0. Validate Security Assignment Access (Original or Substitute)
            bool isOriginalAssignee = assignment.PatrolAssignmentSecurities != null && 
                                      assignment.PatrolAssignmentSecurities.Any(s => s.SecurityId == security.Id);
            
            bool isActiveSubstitute = assignment.PatrolShiftReplacements != null &&
                                      assignment.PatrolShiftReplacements.Any(r => 
                                          r.SubstituteSecurityId == security.Id && 
                                          nowUtc.Date >= r.ReplacementStartDate.Date && 
                                          nowUtc.Date <= r.ReplacementEndDate.Date &&
                                          r.Status != 0); // Assuming Status != 0 for active

            if (!isOriginalAssignee && !isActiveSubstitute)
            {
                throw new UnauthorizedAccessException("You are not assigned to this patrol assignment today.");
            }

            // Also: if Original Assignee is substituted today, they shouldn't be allowed
            bool isCurrentlyReplaced = assignment.PatrolShiftReplacements != null &&
                                       assignment.PatrolShiftReplacements.Any(r => 
                                           r.OriginalSecurityId == security.Id &&
                                           nowUtc.Date >= r.ReplacementStartDate.Date && 
                                           nowUtc.Date <= r.ReplacementEndDate.Date &&
                                           r.Status != 0);

            if (isOriginalAssignee && isCurrentlyReplaced)
            {
                throw new BusinessException("You have been substituted for this assignment today. You cannot start the patrol.");
            }

            // 1. Validate Date Range (StartDate and EndDate)
            if (assignment.StartDate.HasValue && nowUtc < assignment.StartDate.Value)
                throw new BusinessException($"Patrol Assignment has not started yet. Starts at: {assignment.StartDate.Value:yyyy-MM-dd HH:mm} UTC");

            if (assignment.EndDate.HasValue && nowUtc > assignment.EndDate.Value)
                throw new BusinessException($"Patrol Assignment has expired. Ended at: {assignment.EndDate.Value:yyyy-MM-dd HH:mm} UTC");

            // 2. Validate TimeGroup and TimeBlocks
            if (assignment.TimeGroup != null && assignment.TimeGroup.TimeBlocks != null && assignment.TimeGroup.TimeBlocks.Any())
            {
                var timeBlocks = assignment.TimeGroup.TimeBlocks;
                var currentDayOfWeek = nowUtc.DayOfWeek;
                var currentTimeOfDay = nowUtc.TimeOfDay;

                // Check if there is any TimeBlock that matches the current day and time
                var activeBlock = timeBlocks.FirstOrDefault(tb => 
                    tb.DayOfWeek == currentDayOfWeek &&
                    tb.StartTime.HasValue && tb.EndTime.HasValue &&
                    currentTimeOfDay >= tb.StartTime.Value && 
                    currentTimeOfDay <= tb.EndTime.Value
                );

                if (activeBlock == null)
                {
                    throw new BusinessException("Current time is outside the scheduled Time Group for this assignment.");
                }

                // Verify if user already patrolled THIS specific time block for today
                var hasPatrolledBlock = await _repo.HasPatrolledTimeBlockAsync(
                    security.Id, 
                    assignment.Id, 
                    nowUtc, 
                    activeBlock.StartTime!.Value, 
                    activeBlock.EndTime!.Value
                );

                if (hasPatrolledBlock)
                {
                    throw new BusinessException("You have already completed a patrol session for this scheduled shift today.");
                }
            }

            // 3. Prevent Double-Start (Check for existing active session)
            var hasActiveSession = await _repo.HasActiveSessionAsync(security.Id, assignment.Id);
            if (hasActiveSession)
                throw new BusinessException("You already have an active patrol session for this assignment. Please complete it first.");

            var routeAreas = await _repo.GetPatrolRouteAreasAsync(assignment.PatrolRouteId!.Value);

            // 4. Validate Route Areas (Checkpoints must not be empty)
            if (routeAreas == null || !routeAreas.Any())
                throw new BusinessException("Cannot start patrol: The assigned Route does not have any active checkpoints mapped.");

            var patrolSession = new PatrolSession
            {
                PatrolAssignmentId = assignment.Id,
                PatrolAssignmentNameSnap = assignment.Name,

                PatrolRouteId = assignment.PatrolRouteId!.Value,
                PatrolRouteNameSnap = assignment.PatrolRoute?.Name,

                TimeGroupId = assignment.TimeGroupId,
                TimeGroupNameSnap = assignment.TimeGroup?.Name,

                SecurityId = security.Id,
                SecurityNameSnap = security.Name,
                SecurityIdentityIdSnap = security.IdentityId,
                SecurityCardNumberSnap = security.CardNumber,

                StartedAt = nowUtc,
                ApplicationId = AppId,
                CreatedBy = UsernameFormToken,
                CreatedAt = nowUtc,
                UpdatedBy = UsernameFormToken,
                UpdatedAt = nowUtc
            };

            await _repo.AddAsync(patrolSession);

            // -------------------------------------------------------------
            // CYCLE & TRIP CALCULATION LOGIC
            // -------------------------------------------------------------
            var checkpointLogs = new List<PatrolCheckpointLog>();
            int inputCycles = assignment.CycleCount > 0 ? assignment.CycleCount : 1;
            
            // Calculate actual physical trips (perjalanan fisik) through the route areas
            // - FullCycle (1): 1 Cycle = 1 Round Trip (Berangkat A-B-C & Pulang C-B-A) -> 2 Trips
            // - HalfCycle (0): 1 Cycle = 1 One-Way Trip (Berangkat A-B-C saja)       -> 1 Trip
            int totalTrips = assignment.CycleType == PatrolCycleType.FullCycle 
                             ? inputCycles * 2 
                             : inputCycles;

            int globalOrderIndex = 1;

            for (int currentTrip = 1; currentTrip <= totalTrips; currentTrip++)
            {
                // Pengecekan Arah Jalan (Direction): 
                // Trip Ganjil (1, 3, 5) = Jalan Maju (Ascending: A-B-C)
                // Trip Genap  (2, 4, 6) = Jalan Mundur (Descending / Reversed: C-B-A)
                bool isReturnTrip = (currentTrip % 2 == 0);
                
                var orderedAreas = isReturnTrip 
                    ? routeAreas.OrderByDescending(x => x.OrderIndex) 
                    : routeAreas.OrderBy(x => x.OrderIndex);

                foreach (var area in orderedAreas)
                {
                    checkpointLogs.Add(new PatrolCheckpointLog
                    {
                        PatrolSessionId = patrolSession.Id,
                        PatrolAreaId = area.PatrolAreaId,
                        AreaNameSnap = area.PatrolArea?.Name,
                        OrderIndex = globalOrderIndex++,
                        DistanceFromPrevMeters = area.EstimatedDistance,
                        MinDwellTime = area.MinDwellTime,
                        MaxDwellTime = area.MaxDwellTime,
                        CheckpointStatus = PatrolCheckpointStatus.AutoDetected,
                        ArrivedAt = null,
                        LeftAt = null,
                        ApplicationId = patrolSession.ApplicationId,
                        Status = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken
                    });
                }
            }

            

            await _repo.AddCheckpointLogsAsync(checkpointLogs);
            _audit.Action(
                AuditEmitter.AuditAction.SESSION_START,
                "Session",
                "Session Started",
                new {
                    securityId = security.Id,
                    security = security.Name
                }
            );
            _mqttQueue.Enqueue("patrol/session/started", JsonSerializer.Serialize(new {
            sessionId = patrolSession.Id.ToString(),
            securityId = security.Id.ToString()
        }));
            


            return await _repo.GetByIdAsync(patrolSession.Id)
                ?? throw new Exception("Failed to load created PatrolSession");
        }
    public async Task<PatrolSessionRead> StopAsync(Guid id)
    {
        var session = await _repo.GetByIdEntityAsync(id)
            ?? throw new NotFoundException($"Patrol Session with id {id} not found");

        var security = await _repo.GetSecurityByEmail()
            ?? throw new UnauthorizedAccessException("Security not found");

        if (session.SecurityId != security.Id)
            throw new UnauthorizedAccessException("You are not allowed to stop this session");

        if (session.EndedAt != null)
            throw new BusinessException($"Patrol Session with id {id} already stopped");

        // Automatically mark all untouched checkpoints as Missed
        var unhandledLogs = session.PatrolCheckpointLogs
            .Where(log => log.CheckpointStatus == PatrolCheckpointStatus.AutoDetected)
            .ToList();

        foreach (var log in unhandledLogs)
        {
            log.CheckpointStatus = PatrolCheckpointStatus.Missed;
            // ClearedAt remains null because the security guard never interacted with this checkpoint
        }

        session.EndedAt = DateTime.UtcNow;

        SetUpdateAudit(session);
        await _repo.UpdateAsync(session);
        _audit.Action(
                AuditEmitter.AuditAction.SESSION_STOP,
                "Session",
                "Session Stopped",
                new {
                    securityId = security.Id,
                    security = security.Name
                }
            );
        _mqttQueue.Enqueue("patrol/session/ended", "");


        return await _repo.GetByIdAsync(session.Id)
            ?? throw new Exception("Failed to load stopped PatrolSession");
    }

    public async Task<object> SubmitCheckpointActionAsync(PatrolCheckpointActionDto dto)
    {
        // 1. Validate active checkpoint presence
        var log = await _repo.GetActiveCheckpointLogAsync(dto.PatrolCheckpointLogId, dto.PatrolAreaId);
        if (log == null)
        {
            throw new BusinessException("Valid Checkpoint Log not found. Security might not have arrived, or has already left this area.");
        }

        // 1.5 Dwell Time validation: Ensure guard has been at checkpoint for a minimum time
        // Only enforce if the assignment is configured with duration requirement
        if (log.PatrolSession?.PatrolAssignment?.DurationType == PatrolDurationType.WithDuration)
        {
            // Use MinDwellTime from the Checkpoint Log (which was copied from the Route Area), otherwise default to 30 seconds
            int minimumDwellSeconds = log.MinDwellTime ?? 30;
            
            if (log.ArrivedAt.HasValue && (DateTime.UtcNow - log.ArrivedAt.Value).TotalSeconds < minimumDwellSeconds)
            {
                throw new BusinessException($"Dwell time requirement not met. Security must stay at the checkpoint for at least {minimumDwellSeconds} seconds.");
            }
            
            // Note: MaxDwellTime logic is deliberately omitted from throwing runtime exceptions. 
            // It is strictly intended as an indicator flag for analytics/reporting at the end of the session.
        }

        // 1.8 Sequence Validation: Ensure no previous checkpoints were skipped
        if (log.OrderIndex.HasValue && log.PatrolSessionId.HasValue)
        {
            bool hasUncleared = await _repo.HasUnclearedPreviousCheckpointsAsync(log.PatrolSessionId.Value, log.OrderIndex.Value);
            if (hasUncleared)
            {
                throw new BusinessException("Sequence validation failed. You must complete the previous checkpoints in this patrol route before submitting this one.");
            }
        }

        // 2. Mark the log as handled
        log.Notes = dto.SecurityNote;
        log.ClearedAt = DateTime.UtcNow;

        if (dto.PatrolCheckpointStatus == PatrolCheckpointStatus.Cleared)
        {
            log.CheckpointStatus = PatrolCheckpointStatus.Cleared;
        }
        else if (dto.PatrolCheckpointStatus == PatrolCheckpointStatus.HasCase)
        {
            // FIX: Require CaseDetails when HasCase status is selected
            if (dto.CaseDetails == null)
            {
                throw new BusinessException("CaseDetails are required when submitting a checkpoint with HasCase status.");
            }

            log.CheckpointStatus = PatrolCheckpointStatus.HasCase;

            // Assign session and location dynamically
            dto.CaseDetails.PatrolSessionId = log.PatrolSessionId;
            dto.CaseDetails.PatrolAreaId = log.PatrolAreaId;

            // Directly pass frontend's native case details to Case Service (1-action flow)
            await _caseService.CreateAsync(dto.CaseDetails);
        }
        else
        {
            throw new BusinessException("Invalid ActionStatus provided.");
        }

        await _repo.UpdateCheckpointLogAsync(log);

        _audit.Action(
            AuditEmitter.AuditAction.ACTION,
            "PatrolCheckpointLog",
            $"Checkpoint {log.CheckpointStatus}",
            new {
                checkpointLogId = log.Id,
                status = log.CheckpointStatus.ToString()
            }
        );

        return new { success = true, CheckpointStatus = log.CheckpointStatus.ToString() };
    }
}
}
