using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Extension.RootExtension;
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
    public class PatrolCaseService : BaseService, IPatrolCaseService
    {
        private readonly PatrolCaseRepository _repo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditEmitter _audit;
        private readonly IUserService _userService;


        public PatrolCaseService(
            PatrolCaseRepository repo,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService) : base(httpContextAccessor)
        {
            _repo = repo;
            _mapper = mapper;
            _audit = audit;
            _userService = userService;
        }
        public async Task<object> FilterAsync(
            DataTablesProjectedRequest request,
            PatrolCaseFilter filter
        )
        {
            // 1. Map Standard DataTables params
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "UpdatedAt";
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

            var (data, total, filtered) = await _repo.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        public async Task<PatrolCaseRead?> GetByIdAsync(Guid id)
        {
            var patrolCase = await _repo.GetByIdAsync(id);
            if (patrolCase == null)
                throw new NotFoundException($"patrolCase with id {id} not found");
            return patrolCase;
        }

        public async Task<IEnumerable<PatrolCaseRead>> GetAllAsync()
        {
            var patrolCases = await _repo.GetAllAsync();
            return patrolCases;

        }

        // EF STYLE
        public async Task<PatrolCaseRead?> CreateAsync(PatrolCaseCreateDto dto)
        {
            var session = await _repo.GetPatrolSessionAsync(dto.PatrolSessionId.Value)
                ?? throw new NotFoundException(
                    $"PatrolSession with id {dto.PatrolSessionId} not found");

            var patrolCase = _mapper.Map<PatrolCase>(dto);

            // =============================
            // 🔹 SNAPSHOT DARI SESSION
            // =============================
            SetCreateAudit(patrolCase);
            patrolCase.PatrolSessionId = session.Id;
            patrolCase.SecurityId = session.SecurityId;
            patrolCase.PatrolAssignmentId = session.PatrolAssignmentId;
            patrolCase.PatrolRouteId = session.PatrolRouteId;

            // Resolve approval mode based on CaseType, Assignment.ApprovalType, and ThreatLevel
            var assignmentApprovalType = session.PatrolAssignment?.ApprovalType ?? PatrolApprovalType.ByThreatLevel;
            PatrolApprovalType resolvedApprovalType;

            if (dto.CaseType == CaseType.PatrolSummary)
            {
                // Rule 1: PatrolSummary - auto OR, ignore ThreatLevel even if provided
                // Use Assignment type if not ByThreatLevel, otherwise fallback to OR
                resolvedApprovalType = assignmentApprovalType == PatrolApprovalType.ByThreatLevel
                    ? PatrolApprovalType.Or  // Auto OR for PatrolSummary
                    : assignmentApprovalType;

                patrolCase.ThreatLevel = null; // PatrolSummary doesn't use ThreatLevel
            }
            else
            {
                // Rule 2: Operational cases (Incident, Hazard, Damage, Theft, Report)
                // ThreatLevel is REQUIRED for all operational cases
                if (!dto.ThreatLevel.HasValue)
                    throw new BusinessException(
                        "ThreatLevel is required for operational cases (Incident, Hazard, Damage, Theft, Report)");

                if (assignmentApprovalType == PatrolApprovalType.ByThreatLevel)
                {
                    // Rule 3: Assignment is ByThreatLevel - resolve from ThreatLevel
                    resolvedApprovalType = dto.ThreatLevel.Value switch
                    {
                        ThreatLevel.Low => PatrolApprovalType.WithoutApproval,
                        ThreatLevel.Medium => PatrolApprovalType.Or,
                        ThreatLevel.High => PatrolApprovalType.And,
                        ThreatLevel.Critical => PatrolApprovalType.Sequential,
                        _ => PatrolApprovalType.Or
                    };
                }
                else
                {
                    // Rule 4: Assignment has specific approval type - use it
                    // ThreatLevel is still stored for reference, but doesn't affect approval
                    resolvedApprovalType = assignmentApprovalType;
                }

                patrolCase.ThreatLevel = dto.ThreatLevel.Value;
            }

            patrolCase.ApprovalType = resolvedApprovalType;
            var creatorSecurity = await _repo.GetSecurityByIdAsync(session.SecurityId)
                ?? throw new NotFoundException($"Security with id {session.SecurityId} not found");

            patrolCase.SecurityHead1Id = creatorSecurity.SecurityHead1Id;
            patrolCase.SecurityHead2Id = creatorSecurity.SecurityHead2Id;
            patrolCase.CaseStatus = resolvedApprovalType == PatrolApprovalType.WithoutApproval
                ? CaseStatus.Approved
                : CaseStatus.Submitted;
            if (dto.Attachments?.Any() == true)
            {
                foreach (var att in dto.Attachments)
                {
                    patrolCase.PatrolCaseAttachments.Add(new PatrolCaseAttachment
                    {
                        PatrolCaseId = patrolCase.Id,
                        FileUrl = att.FileUrl,
                        FileType = att.FileType,
                        MimeType = MimeTypeHelper.GetMimeType(att.FileUrl),
                        ApplicationId = AppId,
                        UploadedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = UsernameFormToken,
                        UpdatedBy = UsernameFormToken
                    });
                }
            }

            await _repo.AddAsync(patrolCase);
             _audit.Created(
                "Patrol Case",
                patrolCase.Id,
                "Created Patrol Case",
                new { patrolCase.CaseType }
            );

            var result = await _repo.GetByIdAsync(patrolCase.Id);
            return result;
        }

        public async Task<PatrolCaseRead> UpdateAsync(Guid id, PatrolCaseUpdateDto dto)
        {
            PatrolCase? patrolCase = null;

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                // =====================================================
                // 🔹 LOAD TRACKED ENTITY
                // =====================================================
                patrolCase = await _repo.GetByIdEntityAsync(id)
                    ?? throw new NotFoundException($"PatrolCase with id {id} not found");

                // =====================================================
                // 🔹 VALIDASI STATUS - HANYA SUBMITTED/REJECTED YG BOLEH EDIT
                // =====================================================
                if (patrolCase.CaseStatus != CaseStatus.Submitted &&
                    patrolCase.CaseStatus != CaseStatus.Rejected)
                    throw new BusinessException(
                        $"Only Submitted or Rejected cases can be updated. Current status: {patrolCase.CaseStatus}");

                // =====================================================
                // 🔹 UPDATE SCALAR (MANUAL, BIAR JELAS)
                // =====================================================
                if (dto.Title != null)
                    patrolCase.Title = dto.Title;

                if (dto.Description != null)
                    patrolCase.Description = dto.Description;

                if (dto.CaseType.HasValue)
                    patrolCase.CaseType = dto.CaseType.Value;

                SetUpdateAudit(patrolCase);

                // =====================================================
                // 🔥 APPEND ATTACHMENTS (STORAGE BERSAMA)
                // =====================================================
                if (dto.Attachments?.Any() == true)
                {
                    // APPEND only - don't delete existing attachments
                    var newAttachments = dto.Attachments.Select(att =>
                        new PatrolCaseAttachment
                        {
                            PatrolCaseId = patrolCase.Id,
                            FileUrl = att.FileUrl,
                            FileType = att.FileType,
                            MimeType = MimeTypeHelper.GetMimeType(att.FileUrl),
                            ApplicationId = patrolCase.ApplicationId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = UsernameFormToken,
                            UpdatedBy = UsernameFormToken
                        }).ToList();

                    await _repo.AddManyAsync(newAttachments);
                }

                // =====================================================
                // 🔹 SAVE PARENT (TRACKED)
                // =====================================================
                await _repo.UpdateAsync(patrolCase);
            });

            // =====================================================
            // 🔹 RELOAD FOR RESPONSE
            // =====================================================
            var result = await _repo.GetByIdAsync(patrolCase!.Id)
                ?? throw new Exception("Failed to reload PatrolCase after update");

             _audit.Updated(
                "Patrol Case",
                result.Id,
                "Updated Patrol Case",
                new { result.Title });

            return result;
        }

        public async Task DeleteAsync(Guid id)
        {
            var patrolCase = await _repo.GetByIdEntityAsync(id);
            if (patrolCase == null)
                throw new NotFoundException($"PatrolCase with id {id} not found");
            SetDeleteAudit(patrolCase);
             _audit.Deleted(
                "Patrol Area",
                patrolCase.Id,
                "Deleted patrolArea",
                new { patrolCase.Title }
            );
            await _repo.DeleteAsync(id);
        }

        public async Task<PatrolCaseRead> ApproveAsync(Guid id, PatrolCaseApprovalDto dto)
        {
            // Cek permission menggunakan extension method
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasPatrolApprovalPermission())
                throw new UnauthorizedException("User does not have patrol approval permission");

            // Get current user's security ID
            var currentUserEmail = EmailFormToken;
            var currentSecurityId = await _repo.GetSecurityIdByEmailAsync(currentUserEmail);
            var isPrimaryAdmin = IsPrimaryAdminOrHigher();

            PatrolCase? patrolCase = null;

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                // Load entity - use GetByIdEntityForApprovalAsync to allow PrimaryAdmin access
                patrolCase = await _repo.GetByIdEntityForApprovalAsync(id)
                    ?? throw new NotFoundException($"PatrolCase with id {id} not found");

                if (patrolCase.ApprovalType == PatrolApprovalType.WithoutApproval)
                    throw new BusinessException("This case does not require approval.");

                // Validate status - only Submitted can be approved
                if (patrolCase.CaseStatus != CaseStatus.Submitted)
                    throw new BusinessException(
                        $"Only Submitted cases can be approved. Current status: {patrolCase.CaseStatus}");

                var head1Id = patrolCase.SecurityHead1Id;
                var head2Id = patrolCase.SecurityHead2Id;
                var headsMissing = patrolCase.ApprovalType == PatrolApprovalType.Or
                    ? !head1Id.HasValue && !head2Id.HasValue
                    : !head1Id.HasValue || !head2Id.HasValue;

                if (headsMissing)
                {
                    if (!isPrimaryAdmin)
                        throw new UnauthorizedException("Only PrimaryAdmin can approve cases without assigned heads.");

                    patrolCase.CaseStatus = CaseStatus.Approved;
                    patrolCase.ApprovedByHead1Id ??= currentSecurityId;
                    patrolCase.ApprovedByHead1At ??= DateTime.UtcNow;
                    SetUpdateAudit(patrolCase);
                    await _repo.UpdateAsync(patrolCase);
                    return;
                }

                var isHead1 = head1Id.HasValue && head1Id.Value == currentSecurityId;
                var isHead2 = head2Id.HasValue && head2Id.Value == currentSecurityId;

                if (!isHead1 && !isHead2)
                    throw new UnauthorizedException("Only assigned heads can approve this case.");

                if (patrolCase.ApprovalType == PatrolApprovalType.Sequential
                    && isHead2
                    && !patrolCase.ApprovedByHead1Id.HasValue)
                {
                    throw new BusinessException("Head 1 must approve before Head 2.");
                }

                if (isHead1)
                {
                    patrolCase.ApprovedByHead1Id = currentSecurityId;
                    patrolCase.ApprovedByHead1At ??= DateTime.UtcNow;
                }
                if (isHead2)
                {
                    patrolCase.ApprovedByHead2Id = currentSecurityId;
                    patrolCase.ApprovedByHead2At ??= DateTime.UtcNow;
                }

                switch (patrolCase.ApprovalType)
                {
                    case PatrolApprovalType.Or:
                        patrolCase.CaseStatus = CaseStatus.Approved;
                        break;
                    case PatrolApprovalType.And:
                        if (patrolCase.ApprovedByHead1Id.HasValue && patrolCase.ApprovedByHead2Id.HasValue)
                            patrolCase.CaseStatus = CaseStatus.Approved;
                        break;
                    case PatrolApprovalType.Sequential:
                        if (patrolCase.ApprovedByHead1Id.HasValue && patrolCase.ApprovedByHead2Id.HasValue)
                            patrolCase.CaseStatus = CaseStatus.Approved;
                        break;
                    default:
                        throw new BusinessException("Invalid approval type.");
                }
                SetUpdateAudit(patrolCase);

                await _repo.UpdateAsync(patrolCase);
            });

            var result = await _repo.GetByIdForApprovalAsync(patrolCase!.Id)
                ?? throw new Exception("Failed to reload PatrolCase after approve");

             _audit.Updated(
                "Patrol Case",
                result.Id,
                $"Approved{(string.IsNullOrEmpty(dto.Reason) ? "" : $" - {dto.Reason}")}",
                new
                {
                    result.CaseStatus,
                    result.ApprovalType,
                    result.ApprovedByHead1Id,
                    result.ApprovedByHead2Id,
                    result.Title
                });

            return result;
        }

        public async Task<PatrolCaseRead> RejectAsync(Guid id, PatrolCaseApprovalDto dto)
        {
            // Validate reason is required
            if (string.IsNullOrEmpty(dto.Reason))
                throw new BusinessException("Reason is required when rejecting a case");

            // Get current user's security ID
            var currentUserEmail = EmailFormToken;
            var currentSecurityId = await _repo.GetSecurityIdByEmailAsync(currentUserEmail);
            var isPrimaryAdmin = IsPrimaryAdminOrHigher();

            PatrolCase? patrolCase = null;

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                // Load entity - use GetByIdEntityForApprovalAsync to allow PrimaryAdmin access
                patrolCase = await _repo.GetByIdEntityForApprovalAsync(id)
                    ?? throw new NotFoundException($"PatrolCase with id {id} not found");

                if (patrolCase.ApprovalType == PatrolApprovalType.WithoutApproval)
                    throw new BusinessException("This case does not require approval.");

                // Validate status - only Submitted can be rejected
                if (patrolCase.CaseStatus != CaseStatus.Submitted)
                    throw new BusinessException(
                        $"Only Submitted cases can be rejected. Current status: {patrolCase.CaseStatus}");

                var head1Id = patrolCase.SecurityHead1Id;
                var head2Id = patrolCase.SecurityHead2Id;
                var headsMissing = patrolCase.ApprovalType == PatrolApprovalType.Or
                    ? !head1Id.HasValue && !head2Id.HasValue
                    : !head1Id.HasValue || !head2Id.HasValue;

                if (headsMissing)
                {
                    if (!isPrimaryAdmin)
                        throw new UnauthorizedException("Only PrimaryAdmin can reject cases without assigned heads.");

                    patrolCase.CaseStatus = CaseStatus.Rejected;
                    patrolCase.ApprovedByHead1Id ??= currentSecurityId;
                    patrolCase.ApprovedByHead1At ??= DateTime.UtcNow;
                    SetUpdateAudit(patrolCase);
                    await _repo.UpdateAsync(patrolCase);
                    return;
                }

                var isHead1 = head1Id.HasValue && head1Id.Value == currentSecurityId;
                var isHead2 = head2Id.HasValue && head2Id.Value == currentSecurityId;

                if (!isHead1 && !isHead2)
                    throw new UnauthorizedException("Only assigned heads can reject this case.");

                patrolCase.CaseStatus = CaseStatus.Rejected;
                if (isHead1)
                {
                    patrolCase.ApprovedByHead1Id = currentSecurityId;
                    patrolCase.ApprovedByHead1At ??= DateTime.UtcNow;
                }
                if (isHead2)
                {
                    patrolCase.ApprovedByHead2Id = currentSecurityId;
                    patrolCase.ApprovedByHead2At ??= DateTime.UtcNow;
                }
                SetUpdateAudit(patrolCase);

                await _repo.UpdateAsync(patrolCase);
            });

            var result = await _repo.GetByIdForApprovalAsync(patrolCase!.Id)
                ?? throw new Exception("Failed to reload PatrolCase after reject");

             _audit.Updated(
                "Patrol Case",
                result.Id,
                $"Rejected - {dto.Reason}",
                new
                {
                    result.CaseStatus,
                    result.ApprovalType,
                    result.ApprovedByHead1Id,
                    result.ApprovedByHead2Id,
                    result.Title
                });

            return result;
        }

        public async Task<PatrolCaseRead> CloseAsync(Guid id, PatrolCaseCloseDto dto)
        {
            PatrolCase? patrolCase = null;

            await _repo.ExecuteInTransactionAsync(async () =>
            {
                // Load entity - use GetByIdEntityForApprovalAsync to allow PrimaryAdmin access
                patrolCase = await _repo.GetByIdEntityForApprovalAsync(id)
                    ?? throw new NotFoundException($"PatrolCase with id {id} not found");

                // Validate status
                if (patrolCase.CaseStatus != CaseStatus.Approved)
                    throw new BusinessException(
                        $"Only Approved cases can be closed. Current status: {patrolCase.CaseStatus}");

                // Update status
                patrolCase.CaseStatus = CaseStatus.Closed;
                SetUpdateAudit(patrolCase);

                await _repo.UpdateAsync(patrolCase);
            });

            var result = await _repo.GetByIdForApprovalAsync(patrolCase!.Id)
                ?? throw new Exception("Failed to reload PatrolCase after close");

             _audit.Updated(
                "Patrol Case",
                result.Id,
                $"Closed{(string.IsNullOrEmpty(dto.Notes) ? "" : $" - {dto.Notes}")}",
                new { result.CaseStatus, result.Title });

            return result;
        }

        public async Task DeleteAttachmentAsync(Guid caseId, Guid attachmentId)
        {
            var patrolCase = await _repo.GetByIdAsync(caseId)
                ?? throw new NotFoundException($"PatrolCase with id {caseId} not found");

            // Only allow attachment deletion for Submitted or Rejected cases
            if (patrolCase.CaseStatus != CaseStatus.Submitted && patrolCase.CaseStatus != CaseStatus.Rejected)
                throw new BusinessException(
                    $"Cannot delete attachment. Case status is {patrolCase.CaseStatus}");

            var deleted = await _repo.DeleteAttachmentAsync(caseId, attachmentId);

            if (!deleted)
                throw new NotFoundException($"Attachment with id {attachmentId} not found");

            _audit.Deleted(
                "Patrol Case Attachment",
                attachmentId,
                $"Deleted attachment from case: {patrolCase.Title}",
                new { caseId, attachmentId });
        }

        private bool IsPrimaryAdminOrHigher()
        {
            var role = Http.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role))
                return false;

            return role.Equals(LevelPriority.PrimaryAdmin.ToString(), StringComparison.OrdinalIgnoreCase)
                || role.Equals(LevelPriority.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase)
                || role.Equals(LevelPriority.System.ToString(), StringComparison.OrdinalIgnoreCase);
        }

    }
}
