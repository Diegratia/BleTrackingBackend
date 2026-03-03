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
        private readonly IAuditEmitter _audit;
        private readonly IMqttPubQueue _mqttQueue;

        public PatrolSessionService(
            PatrolSessionRepository repo,
            IAuditEmitter audit,
            IMqttPubQueue mqttQueue,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
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
            };
            patrolSession = await _repo.AddAsync(patrolSession);

            var checkpointLogs = routeAreas.Select(area => new PatrolCheckpointLog
            {
                PatrolSessionId = patrolSession.Id,
                PatrolAreaId = area.PatrolAreaId,
                AreaNameSnap = area.PatrolArea?.Name,
                OrderIndex = area.OrderIndex,
                DistanceFromPrevMeters = area.EstimatedDistance,
                ArrivedAt = null,
                LeftAt = null,
                ApplicationId = patrolSession.ApplicationId,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            

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
       // MASIH KURANG PENGECEKAN PATROL CASE - KARENA WAJIB BUAT PATROL CASE TIAP SELESAI 
    public async Task<PatrolSessionRead> StopAsync(Guid sessionId)
    {

        var session = await _repo.GetByIdEntityAsync(sessionId);

        if (session == null)
            throw new NotFoundException("PatrolSession not found");

        var security = await _repo.GetSecurityByEmail();
        if (security == null)
            throw new UnauthorizedAccessException("Security not found");

        if (session.SecurityId != security.Id)
            throw new UnauthorizedAccessException("You are not allowed to stop this session");

        if (session.EndedAt.HasValue)
            throw new BusinessException("PatrolSession already ended");

        session.EndedAt = DateTime.UtcNow;

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
}


    }
