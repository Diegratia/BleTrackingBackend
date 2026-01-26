using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Repositories.Repository.RepoModel;

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
            var (data, total) = await _repo.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = total,
                data
            };
        }

   
    }
}