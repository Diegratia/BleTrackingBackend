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

        public async Task<PatrolAssignmentDto?> GetByIdAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");
            return patrolAssignment == null ? null : _mapper.Map<PatrolAssignmentDto>(patrolAssignment);
        }

        public async Task<IEnumerable<PatrolAssignmentDto>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PatrolAssignmentDto>>(patrolAreas);
        }

        public async Task<PatrolAssignmentDto> CreateAsync(PatrolAssignmentCreateDto createDto)
        {
            if (!await _repository.TimeGroupExistsAsync(createDto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup with id {createDto.TimeGroupId} not found");

            if (!await _repository.PatrolRouteExistsAsync(createDto.PatrolRouteId!.Value))
                throw new NotFoundException($"PatrolRoute with id {createDto.PatrolRouteId} not found");

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
            await _repository.AddAsync(patrolAssignment);
            await _audit.Created(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Created patrolAssignment",
                new { patrolAssignment.Name }
            );
            return _mapper.Map<PatrolAssignmentDto>(patrolAssignment);
        }

        public async Task<PatrolAssignmentDto> UpdateAsync(Guid id, PatrolAssignmentUpdateDto updateDto)
        {
            var patrolAssignment = await _repository.GetByIdAsync(id);
                  if (!await _repository.TimeGroupExistsAsync(updateDto.TimeGroupId!.Value))
                throw new NotFoundException($"TimeGroup with id {updateDto.TimeGroupId} not found");

            if (!await _repository.PatrolRouteExistsAsync(updateDto.PatrolRouteId!.Value))
                throw new NotFoundException($"PatrolRoute with id {updateDto.PatrolRouteId} not found");

            var missingSecurityIds = await _repository
                .GetMissingSecurityIdsAsync(updateDto.SecurityIds ?? new List<Guid>());

            if (missingSecurityIds.Count > 0)
            {
                throw new NotFoundException(
                    $"Security not found: {string.Join(", ", missingSecurityIds)}"
                );
            }

            SetUpdateAudit(patrolAssignment);
            _mapper.Map(updateDto, patrolAssignment);
            await _repository.UpdateAsync(patrolAssignment);
            await _audit.Updated(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Updated patrolAssignment",
                new { patrolAssignment.Name }
            );
            return _mapper.Map<PatrolAssignmentDto>(patrolAssignment);
        }

        public async Task DeleteAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");
            SetDeleteAudit(patrolAssignment);
            await _audit.Deleted(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Deleted patrolAssignment",
                new { patrolAssignment.Name }
            );
            await _repository.DeleteAsync(id);
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status" };

            var filterService = new GenericDataTableService<PatrolAssignment, PatrolAssignmentDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
            public async Task<IEnumerable<PatrolAssignmentLookUpDto>> GetAllLookUpAsync()
        {
            var patrolareas = await _repository.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<PatrolAssignmentLookUpDto>>(patrolareas);
        }

        
    }
}