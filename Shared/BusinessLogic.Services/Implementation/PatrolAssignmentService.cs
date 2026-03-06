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
using Shared.Contracts;
using Shared.Contracts.Read;
using BusinessLogic.Services.ServiceValidators;

namespace BusinessLogic.Services.Implementation
{
    public class PatrolAssignmentService : BaseService, IPatrolAssignmentService
    {
        private readonly PatrolAssignmentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;
        private readonly IPatrolAssignmentValidatorService _validationService;


        public PatrolAssignmentService(
            PatrolAssignmentRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            IPatrolAssignmentValidatorService validationService
            ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _audit = audit;
            _validationService = validationService;
        }

        public async Task<PatrolAssignmentRead?> GetByIdAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");
            return patrolAssignment;
        }

        public async Task<IEnumerable<PatrolAssignmentRead>> GetAllAsync()
        {
            var patrolAreas = await _repository.GetAllAsync();
            return patrolAreas;
        }

        public async Task<PatrolAssignmentRead> CreateAsync(PatrolAssignmentCreateDto createDto)
        {
            await _validationService.ValidateAssignmentCreateAsync(createDto, AppId);

            var patrolAssignment = _mapper.Map<PatrolAssignment>(createDto);
            
            // Validate and assign Security Heads
            if (createDto.SecurityHead1Id.HasValue && createDto.SecurityHead2Id.HasValue)
            {   
                patrolAssignment.SecurityHead1Id = createDto.SecurityHead1Id.Value;
                patrolAssignment.SecurityHead2Id = createDto.SecurityHead2Id.Value;
            }

            if (!createDto.ApprovalType.HasValue)
                patrolAssignment.ApprovalType = PatrolApprovalType.ByThreatLevel;
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
             _audit.Created(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Created patrolAssignment",
                new { patrolAssignment.Name }
            );
            var result = await _repository.GetByIdAsync(patrolAssignment.Id);
            return result!;
        }

        public async Task<PatrolAssignmentRead> UpdateAsync(Guid id, PatrolAssignmentUpdateDto dto)
        {
            var assignment = await _repository.GetByIdWithTrackingAsync(id)
                ?? throw new NotFoundException($"PatrolAssignment {id} not found");

            await _validationService.ValidateAssignmentUpdateAsync(id, dto, AppId, assignment);

            var newSecurityIds = dto.SecurityIds?
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList() ?? new List<Guid>();


            // 1. Replace all child (SQL-style)
            await _repository.RemoveAllPatrolAssignmentSecurities(id);

            foreach (var secId in dto.SecurityIds!.Distinct())
            {
                await _repository.AddPatrolAssignmentSecurityAsync(
                    new PatrolAssignmentSecurity
                    {
                        PatrolAssignmentId = assignment.Id,
                        SecurityId = secId!.Value,
                        ApplicationId = assignment.ApplicationId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken
                    });
            }

            // 2. Update parent scalar only
            _mapper.Map(dto, assignment);
            if (dto.SecurityHead1Id.HasValue && dto.SecurityHead2Id.HasValue)
            {
                 assignment.SecurityHead1Id = dto.SecurityHead1Id.Value;
                 assignment.SecurityHead2Id = dto.SecurityHead2Id.Value;
            }

            if (!dto.ApprovalType.HasValue)
                assignment.ApprovalType = PatrolApprovalType.WithoutApproval;
            SetUpdateAudit(assignment);

            await _repository.UpdateAsync(assignment);

            var result = await _repository.GetByIdAsync(id);
             _audit.Updated("Patrol Assignment", id, "Updated", new { result!.Name });

            return result!;
        }



        public async Task DeleteAsync(Guid id)
        {
            var patrolAssignment = await _repository.GetByIdWithTrackingAsync(id);
            if (patrolAssignment == null)
                throw new NotFoundException($"PatrolAssignment with id {id} not found");

            await _validationService.EnsureNoActiveSessionsAsync(id);

            await _repository.ExecuteInTransactionAsync(async () =>
            {
                SetDeleteAudit(patrolAssignment);
                patrolAssignment.Status = 0;
                await _repository.RemoveAssignmentSecurities(id);
                await _repository.DeleteAsync(id);
            });
             _audit.Deleted(
                "Patrol Assignment",
                patrolAssignment.Id,
                "Deleted patrolAssignment",
                new { patrolAssignment.Name }
            );
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
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
            filter.SortDir = request.SortDir;
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = data
            };
        }

        public async Task<PatrolShiftReplacementRead> AddShiftReplacementAsync(PatrolShiftReplacementCreateDto createDto)
        {
            var assignment = await _repository.GetByIdWithTrackingAsync(createDto.PatrolAssignmentId);
            if (assignment == null)
                throw new NotFoundException($"PatrolAssignment {createDto.PatrolAssignmentId} not found");

            await _validationService.ValidateShiftReplacementAsync(
                assignment, 
                createDto.OriginalSecurityId, 
                createDto.SubstituteSecurityId, 
                createDto.ReplacementStartDate, 
                createDto.ReplacementEndDate
            );

            var entity = new PatrolShiftReplacement
            {
                PatrolAssignmentId = createDto.PatrolAssignmentId,
                OriginalSecurityId = createDto.OriginalSecurityId,
                SubstituteSecurityId = createDto.SubstituteSecurityId,
                ReplacementStartDate = createDto.ReplacementStartDate,
                ReplacementEndDate = createDto.ReplacementEndDate,
                Reason = createDto.Reason,
                ApplicationId = AppId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = UsernameFormToken,
                UpdatedBy = UsernameFormToken
            };

            await _repository.AddShiftReplacementAsync(entity);
            
            _audit.Created(
                "Shift Replacement",
                entity.Id,
                "Created shift replacement",
                new { assignment.Name, OriginalSecurityId = createDto.OriginalSecurityId, SubstituteSecurityId = createDto.SubstituteSecurityId }
            );

            // Fetch to return DTO properly
            var saved = await _repository.GetShiftReplacementByIdAsync(entity.Id);
            return new PatrolShiftReplacementRead
            {
                Id = saved!.Id,
                PatrolAssignmentId = saved.PatrolAssignmentId,
                OriginalSecurity = saved.OriginalSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.OriginalSecurity.Id,
                    Name = saved.OriginalSecurity.Name,
                    CardNumber = saved.OriginalSecurity.CardNumber,
                    IdentityId = saved.OriginalSecurity.IdentityId,
                },
                SubstituteSecurity = saved.SubstituteSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.SubstituteSecurity.Id,
                    Name = saved.SubstituteSecurity.Name,
                    CardNumber = saved.SubstituteSecurity.CardNumber,
                    IdentityId = saved.SubstituteSecurity.IdentityId,
                },
                ReplacementStartDate = saved.ReplacementStartDate,
                ReplacementEndDate = saved.ReplacementEndDate,
                Reason = saved.Reason
            };
        }

        public async Task RemoveShiftReplacementAsync(Guid id)
        {
            var entity = await _repository.GetShiftReplacementByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolShiftReplacement {id} not found");

            // Prevent deletion if replacement date has started
            if (DateOnly.FromDateTime(DateTime.UtcNow) >= entity.ReplacementStartDate)
            {
                throw new BusinessException($"Cannot delete shift replacement because the replacement date has already started or passed.");
            }

            await _repository.RemoveShiftReplacementAsync(entity);

            _audit.Deleted(
                "Shift Replacement",
                id,
                "Deleted shift replacement",
                new { entity.PatrolAssignmentId }
            );
        }

        public async Task<PatrolShiftReplacementRead> UpdateShiftReplacementAsync(Guid id, PatrolShiftReplacementUpdateDto dto)
        {
            var entity = await _repository.GetShiftReplacementByIdAsync(id);
            if (entity == null)
                throw new NotFoundException($"PatrolShiftReplacement {id} not found");
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            // Prevent update if replacement date has started
            if (today >= entity.ReplacementStartDate)
            {
                throw new BusinessException($"Cannot update shift replacement because the replacement date has already started or passed.");
            }

            var assignment = await _repository.GetByIdWithTrackingAsync(entity.PatrolAssignmentId);
            
            var originalSecurityId = dto.OriginalSecurityId ?? entity.OriginalSecurityId;
            var substituteSecurityId = dto.SubstituteSecurityId ?? entity.SubstituteSecurityId;
            var startDate = dto.ReplacementStartDate ?? entity.ReplacementStartDate;
            var endDate = dto.ReplacementEndDate ?? entity.ReplacementEndDate;

            await _validationService.ValidateShiftReplacementAsync(
                assignment!, 
                originalSecurityId, 
                substituteSecurityId, 
                startDate, 
                endDate, 
                id
            );

            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = UsernameFormToken;

            await _repository.UpdateShiftReplacementAsync(entity);

            _audit.Updated(
                "Shift Replacement",
                id,
                "Updated shift replacement",
                new { entity.PatrolAssignmentId }
            );

            // Fetch to return DTO properly
            var saved = await _repository.GetShiftReplacementByIdAsync(entity.Id);
            return new PatrolShiftReplacementRead
            {
                Id = saved!.Id,
                PatrolAssignmentId = saved.PatrolAssignmentId,
                OriginalSecurity = saved.OriginalSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.OriginalSecurity.Id,
                    Name = saved.OriginalSecurity.Name,
                    CardNumber = saved.OriginalSecurity.CardNumber,
                    IdentityId = saved.OriginalSecurity.IdentityId,
                },
                SubstituteSecurity = saved.SubstituteSecurity == null ? null : new SecurityListRead
                {
                    Id = saved.SubstituteSecurity.Id,
                    Name = saved.SubstituteSecurity.Name,
                    CardNumber = saved.SubstituteSecurity.CardNumber,
                    IdentityId = saved.SubstituteSecurity.IdentityId,
                },
                ReplacementStartDate = saved.ReplacementStartDate,
                ReplacementEndDate = saved.ReplacementEndDate,
                Reason = saved.Reason
            };
        }


    }
}
