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

            if (!await _repository.PatrolRouteExistsAsync(createDto.PatrolRouteId!.Value))
                throw new NotFoundException($"PatrolRoute with id {createDto.PatrolRouteId} not found");

            var invalidSecurityIds =
            await _repository.GetInvalidSecurityIdsByApplicationAsync(createDto.SecurityIds, AppId);
            if (invalidSecurityIds.Any())
            {
                throw new NotFoundException(
                    $"Some SecurityIds do not belong to this Application: {string.Join(", ", invalidSecurityIds)}"
                );
            }

            // var invalidIds = await _repository.GetInvalidOwnershipIdsAsync<MstSecurity>(
            //     createDto.SecurityIds,
            //     AppId
            // );

            // if (invalidIds.Any())
            // {
            //     throw new DataView.ValidationException(new Dictionary<string, string[]>
            //     {
            //         ["securityIds"] = invalidIds.Select(id => $"Invalid Id: {id}").ToArray()
            //     });
            // }


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
            return _mapper.Map<PatrolAssignmentDto>(result);
        }

        //     public async Task<PatrolAssignmentDto> UpdateAsync(
        //     Guid id,
        //     PatrolAssignmentUpdateDto dto)
        // {
        //     PatrolAssignment result = null;

        //         var assignment = await _repository.GetByIdWithTrackingAsync(id)
        //             ?? throw new NotFoundException(
        //                 $"PatrolAssignment with id {id} not found");

        //         // ================= VALIDASI =================

        //         if (dto.PatrolRouteId.HasValue &&
        //             !await _repository.PatrolRouteExistsAsync(dto.PatrolRouteId.Value))
        //             throw new NotFoundException($"PatrolRoute not found");

        //         var newSecurityIds = dto.SecurityIds?
        //             .Where(x => x.HasValue)
        //             .Select(x => x.Value)
        //             .Distinct()
        //             .ToList() ?? new List<Guid>();

        //         var missing =
        //             await _repository.GetMissingSecurityIdsAsync(newSecurityIds);

        //         if (missing.Count > 0)
        //             throw new NotFoundException(
        //                 $"Security not found: {string.Join(", ", missing)}");



        //     // ================= REPLACE SECURITY =================

        //     await _repository.RemoveAllPatrolAssignmentSecurities(id);

        //     _mapper.Map(dto, assignment);

        //     foreach (var secId in newSecurityIds)
        //     {
        //         await _repository.AddPatrolAssignmentSecurityAsync(
        //             new PatrolAssignmentSecurity
        //             {
        //                 PatrolAssignmentId = assignment.Id,
        //                 SecurityId = secId,
        //                 ApplicationId = assignment.ApplicationId,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedAt = DateTime.UtcNow,
        //                 CreatedBy = UsernameFormToken,
        //                 UpdatedBy = UsernameFormToken
        //             });
        //     }

        //         // 🔥 INI WAJIB (SAMA SEPERTI ROUTE)

        //         // ================= UPDATE SCALAR =================
        //         // _mapper.Map(dto, assignment);
        //         SetUpdateAudit(assignment);
        //         await _repository.UpdateAsync(assignment);

        //     result = await _repository.GetByIdAsync(assignment.Id)
        //         ?? throw new Exception("Failed reload");



        //     await _audit.Updated(
        //         "Patrol Assignment",
        //         result.Id,
        //         "Updated Patrol Assignment",
        //         new { result.Name });

        //     return _mapper.Map<PatrolAssignmentDto>(result);
        // }
        
                public async Task<PatrolAssignmentDto> UpdateAsync(Guid id, PatrolAssignmentUpdateDto dto)
        {
            var assignment = await _repository.GetByIdWithTrackingAsync(id)
                ?? throw new NotFoundException($"PatrolAssignment {id} not found");

                // ================= VALIDASI =================

                if (dto.PatrolRouteId.HasValue &&
                    !await _repository.PatrolRouteExistsAsync(dto.PatrolRouteId.Value))
                    throw new NotFoundException($"PatrolRoute not found");

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

            return _mapper.Map<PatrolAssignmentDto>(result);
        }



        public async Task DeleteAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdAsync(id);
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