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

        public async Task<PatrolSessionRead> CreateAsync(PatrolSessionCreateDto dto)
        {
            var patrolSession = _mapper.Map<PatrolSession>(dto);
            patrolSession = await _repo.AddAsync(patrolSession);
            return _mapper.Map<PatrolSessionRead>(patrolSession);
        }
    }
}