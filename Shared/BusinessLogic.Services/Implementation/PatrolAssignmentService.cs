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
using Shared.Contracts.Shared.Contracts;
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
            return patrolAssignment == null ? null : _mapper.Map<PatrolAssignmentRead>(patrolAssignment);
        }

        public async Task<IEnumerable<PatrolAssignmentRead>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PatrolAssignmentRead>>(patrolAreas);
        }

        public async Task<PatrolAssignmentRead> CreateAsync(PatrolAssignmentCreateDto createDto)
        {

            if (!await _repository.PatrolRouteExistsAsync(createDto.PatrolRouteId!.Value))
                throw new NotFoundException($"PatrolRoute with id {createDto.PatrolRouteId} not found");
            if (!await _repository.TimeGroupExistsAsync(createDto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup with id {createDto.TimeGroupId} not found");
            if (!await _repository.CheckTimeGroupPatrolType(createDto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup should be Patrol Type");

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
            await _audit.Created(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Created patrolAssignment",
                new { patrolAssignment.Name }
            );
            var result = await _repository.GetByIdAsync(patrolAssignment.Id);
            return _mapper.Map<PatrolAssignmentRead>(result);
        }

        public async Task<PatrolAssignmentRead> UpdateAsync(Guid id, PatrolAssignmentUpdateDto dto)
        {
            var assignment = await _repository.GetByIdWithTrackingAsync(id)
                ?? throw new NotFoundException($"PatrolAssignment {id} not found");

            // ================= VALIDASI =================

            if (dto.PatrolRouteId.HasValue &&
                !await _repository.PatrolRouteExistsAsync(dto.PatrolRouteId.Value))
                throw new NotFoundException($"PatrolRoute not found");
            if (dto.TimeGroupId.HasValue &&
                !await _repository.TimeGroupExistsAsync(dto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup with id {dto.TimeGroupId} not found");

            var newSecurityIds = dto.SecurityIds?
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList() ?? new List<Guid>();

            var missing =
                await _repository.GetMissingSecurityIdsAsync(newSecurityIds);

            if (missing.Count > 0)
                throw new NotFoundException(
                    $"Security not found: {string.Join(", ", missing)}");

            var invalidSecurityIds =
                await _repository.GetInvalidSecurityIdsByApplicationAsync(newSecurityIds, AppId);
            if (invalidSecurityIds.Any())
            {
                throw new NotFoundException(
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
                        SecurityId = secId.Value,
                        ApplicationId = assignment.ApplicationId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken
                    });
            }

            // 2. Update parent scalar only
            _mapper.Map(dto, assignment);
            SetUpdateAudit(assignment);

            await _repository.UpdateAsync(assignment);

            var result = await _repository.GetByIdAsync(id);
            await _audit.Updated("Patrol Assignment", id, "Updated", new { result!.Name });

            return _mapper.Map<PatrolAssignmentRead>(result);
        }



        public async Task DeleteAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdWithTrackingAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");
            await _repository.ExecuteInTransactionAsync(async () =>
            {
                SetDeleteAudit(patrolAssignment);
                patrolAssignment.Status = 0;
                await _repository.RemoveAssignmentSecurities(id);
                await _repository.DeleteAsync(id);
            });
            await _audit.Deleted(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Deleted patrolAssignment",
                new { patrolAssignment.Name }
            );
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryableWithoutTracking();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status" };

            var filterService = new GenericDataTableService<PatrolAssignment, PatrolAssignmentRead>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
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
            filter.SortColumn = request.SortColumn;
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            // RM → DTO
            var dto = _mapper.Map<List<PatrolAssignmentRead>>(data);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = dto
            };
        }

        
    }
}