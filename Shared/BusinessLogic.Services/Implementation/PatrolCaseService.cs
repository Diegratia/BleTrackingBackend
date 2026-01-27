using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Repositories.Repository.RepoModel;
using Shared.Contracts;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolCaseService : BaseService, IPatrolCaseService
    {
        private readonly PatrolCaseRepository _repo;
        // private readonly PatrolSessionRepository _sessionRepo;
        private readonly MstSecurityRepository _securityRepo;
        private readonly PatrolRouteRepository _routeRepo;
        private readonly PatrolAssignmentRepository _assignmentRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;

        public PatrolCaseService(
            PatrolCaseRepository repo,
            // PatrolSessionRepository sessionRepo,
            MstSecurityRepository securityRepo,
            PatrolRouteRepository routeRepo,
            PatrolAssignmentRepository assignmentRepo,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
        }
        public async Task<object> FilterAsync(
            DataTablesRequest request,
            PatrolCaseFilter filter
        )
        {
            // 1. Map Standard DataTables params
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn;
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            // 2. Map Time Report (Shortcut for DateRange)
            if (!string.IsNullOrEmpty(request.TimeReport))
            {
                // Helper to calculate date range based on "Daily", "Weekly", etc.
                // We can reuse the logic from BaseProjectionRepository or simple switch here.
                // For now, let's assume specific date filters are passed or implement a simple helper if needed.
                // If you have a shared helper, use it. Otherwise, we can infer it or ignored it if specific date filters exist.
            }

            // 3. Map Date Filters (Generic Dictionary -> Specific Prop)
            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("UpdatedAt", out var dateFilter))
                {
                    filter.DateFrom = dateFilter.DateFrom;
                    filter.DateTo = dateFilter.DateTo;
                }
            }

            // 4. Map Specific Filters (Dictionary -> Properties)
            if (request.Filters != null)
            {
                // if (request.Filters.TryGetValue("CaseStatus", out var statusObj) && statusObj != null)
                // {
                //      // Handle simple value or array (if logic supports it)
                //      // For simple single value:
                //      if (int.TryParse(statusObj.ToString(), out int statusInt))
                //         filter.CaseStatus = (CaseStatus)statusInt;
                //      // Note: If you need to support array [3, 1], PatrolCaseFilter.CaseStatus needs to be List<CaseStatus>
                // }

                // if (request.Filters.TryGetValue("CaseType", out var typeObj) && typeObj != null)
                // {
                //      if (int.TryParse(typeObj.ToString(), out int typeInt))
                //         filter.CaseType = (CaseType)typeInt;
                // }

                if (request.Filters.TryGetValue("CaseStatus", out var statusObj) && statusObj != null)
                {
                    // Handle simple value or array (if logic supports it)
                    // For simple single value:
                    if (Enum.TryParse<CaseStatus>(
                            statusObj.ToString(),
                            ignoreCase: true,
                            out var statusEnum))
                    {
                        filter.CaseStatus = statusEnum;
                    }
                    // Note: If you need to support array [3, 1], PatrolCaseFilter.CaseStatus needs to be List<CaseStatus>
                }

                if (request.Filters.TryGetValue("CaseType", out var typeObj) && typeObj != null)
                {
                    if (Enum.TryParse<CaseType>(
                           typeObj.ToString(),
                           ignoreCase: true,
                           out var typeEnum))
                    {
                        filter.CaseType = typeEnum;
                    }
                }

                if (request.Filters.TryGetValue("SecurityId", out var secIdObj) && secIdObj != null)
                {
                    if (Guid.TryParse(secIdObj.ToString(), out Guid secId))
                        filter.SecurityId = secId;
                }

                if (request.Filters.TryGetValue("PatrolAssignmentId", out var assignIdObj) && assignIdObj != null)
                {
                    if (Guid.TryParse(assignIdObj.ToString(), out Guid assignId))
                        filter.PatrolAssignmentId = assignId;
                }

                if (request.Filters.TryGetValue("PatrolRouteId", out var routeIdObj) && routeIdObj != null)
                {
                    if (Guid.TryParse(routeIdObj.ToString(), out Guid routeId))
                        filter.PatrolRouteId = routeId;
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

        public async Task<PatrolCaseDto?> GetByIdAsync(Guid id)
        {
            var patrolCase = await _repo.GetByIdAsync(id);
            if (patrolCase == null)
                throw new NotFoundException($"patrolCase with id {id} not found");
            return patrolCase == null ? null : _mapper.Map<PatrolCaseDto>(patrolCase);
        }

        public async Task<IEnumerable<PatrolCaseDto>> GetAllAsync()
        {
            var patrolCases = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PatrolCaseDto>>(patrolCases);
        }

        public async Task<PatrolCaseDto> CreateAsync(PatrolCaseCreateDto createDto)
        {
            if (!await _repo.SessionExistsAsync(createDto.PatrolSessionId!.Value))
                throw new NotFoundException($"PatrolSessionId with id {createDto.PatrolSessionId} not found");

            var patrolCase = _mapper.Map<PatrolCase>(createDto);
            SetCreateAudit(patrolCase);

            await _repo.AddAsync(patrolCase);
            await _audit.Created(
                "Patrol Case",
                patrolCase.Id,
                "Created Patrol Case",
                new { patrolCase.Title }
            );
            return _mapper.Map<PatrolCaseDto>(patrolCase);
        }
        
        //  public async Task<PatrolCaseDto> UpdateAsync(Guid id, PatrolCaseUpdateDto updateDto)
        // {
        //     var patrolCase = await _repo.GetByIdAsync(id);
        //     if (patrolCase == null)
        //         throw new NotFoundException($"patrolCase with id {id} not found");

        //     // SetUpdateAudit(patrolCase);
        //     _mapper.Map(updateDto, patrolCase);
        //     await _repo.UpdateAsync(patrolCase);
        //     await _audit.Updated(
        //         "Patrol Case",
        //         patrolCase.Id,
        //         "Updated patrolCase",
        //         new { patrolCase.Title }
        //     );
        //     return _mapper.Map<PatrolAreaDto>(patrolArea);
        // }
        
    }
}