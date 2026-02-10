using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private readonly IAuditEmitter _audit;


        public PatrolSessionService(
            PatrolSessionRepository repo,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
            _mapper = mapper;
            _audit = audit;
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

            if (!string.IsNullOrEmpty(request.TimeReport))
            {

            }

            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

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
            var assignment = await _repo.GetAssignmentByIdAsync(dto.PatrolAssignmentId.Value);

            if (assignment == null)
                throw new NotFoundException("PatrolAssignment not found");

            var security = await _repo.GetSecurityByEmail();
            if (security == null)
                throw new UnauthorizedAccessException("Security not found");

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

                StartedAt = DateTime.UtcNow,
            };
            patrolSession = await _repo.AddAsync(patrolSession);

            // Create checkpoint logs for each area in the route
            var routeAreas = await _repo.GetPatrolRouteAreasAsync(patrolSession.PatrolRouteId);

            var checkpointLogs = routeAreas.Select(area => new PatrolCheckpointLog
            {
                PatrolSessionId = patrolSession.Id,
                PatrolAreaId = area.PatrolAreaId,
                AreaNameSnap = area.PatrolArea?.Name,
                OrderIndex = area.OrderIndex,
                DistanceFromPrevMeters = area.EstimatedDistance,
                ArrivedAt = null, // Will be set when security arrives at checkpoint
                LeftAt = null,
                ApplicationId = patrolSession.ApplicationId,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            await _repo.AddCheckpointLogsAsync(checkpointLogs);

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
        throw new UnauthorizedAccessException("You are not allowed to stop this session");

        if (session.EndedAt.HasValue)
            throw new BusinessException("PatrolSession already ended");

        session.EndedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(session);

        return await _repo.GetByIdAsync(session.Id)
            ?? throw new Exception("Failed to load stopped PatrolSession");
    }
}


    }
