using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class EvacuationAlertService : BaseService, IEvacuationAlertService
    {
        private readonly EvacuationAlertRepository _repository;
        private readonly EvacuationTransactionRepository _transactionRepository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;
        private readonly EvacuationMqttService _evacuationMqtt;
        private readonly ILogger<EvacuationAlertService> _logger;

        public EvacuationAlertService(
            EvacuationAlertRepository repository,
            EvacuationTransactionRepository transactionRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            EvacuationMqttService evacuationMqtt,
            ILogger<EvacuationAlertService> logger) : base(httpContextAccessor)
        {
            _repository = repository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _audit = audit;
            _evacuationMqtt = evacuationMqtt;
            _logger = logger;
        }

        public async Task<EvacuationAlertRead> GetByIdAsync(Guid id)
        {
            var alert = await _repository.GetByIdAsync(id);
            if (alert == null)
                throw new NotFoundException($"Evacuation Alert with id {id} not found");
            return alert;
        }

        public async Task<IEnumerable<EvacuationAlertRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<EvacuationAlertRead> CreateAsync(EvacuationAlertCreateDto createDto)
        {
            // Check if there's an active evacuation
            var activeAlert = await _repository.GetActiveAlertAsync(AppId);
            if (activeAlert != null)
                throw new BusinessException("Cannot create new evacuation while another is active or paused");

            var alert = _mapper.Map<EvacuationAlert>(createDto);
            SetCreateAudit(alert);
            alert.Status = 1;
            alert.AlertStatus = EvacuationAlertStatus.Draft;
            alert.ApplicationId = AppId;
            alert.TriggeredBy = UsernameFormToken;

            await _repository.AddAsync(alert);

            _audit.Created(
                "EvacuationAlert",
                alert.Id,
                "Created evacuation alert",
                new { alert.Title, alert.TriggerType }
            );

            return _mapper.Map<EvacuationAlertRead>(alert);
        }

        public async Task UpdateAsync(Guid id, EvacuationAlertUpdateDto updateDto)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new KeyNotFoundException("Evacuation Alert not found");

            // Only allow update if draft
            if (alert.AlertStatus != EvacuationAlertStatus.Draft && alert.AlertStatus != EvacuationAlertStatus.Paused)
                throw new BusinessException("Can only update draft or paused evacuations");

            SetUpdateAudit(alert);
            _mapper.Map(updateDto, alert);
            await _repository.UpdateAsync(alert);

            _audit.Updated(
                "EvacuationAlert",
                alert.Id,
                "Updated evacuation alert",
                new { alert.Title }
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
            {
                throw new KeyNotFoundException("Evacuation Alert not found");
            }

            // Only allow delete if draft
            if (alert.AlertStatus != EvacuationAlertStatus.Draft)
                throw new BusinessException("Can only delete draft evacuations");

            SetDeleteAudit(alert);
            alert.Status = 0;

            await _repository.DeleteAsync(id);

            _audit.Deleted(
                "EvacuationAlert",
                alert.Id,
                "Deleted evacuation alert",
                new { alert.Title }
            );
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, EvacuationAlertFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "CreatedAt";
            filter.SortDir = request.SortDir ?? "desc";
            filter.Search = request.SearchValue;

            var (data, total, filtered) = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        public async Task<EvacuationAlertRead> StartAsync(Guid id)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            if (alert.AlertStatus != EvacuationAlertStatus.Draft && alert.AlertStatus != EvacuationAlertStatus.Paused)
                throw new BusinessException("Can only start draft or paused evacuations");

            // Request total active persons from engine
            int totalRequired = 0;
            try
            {
                totalRequired = await _evacuationMqtt.RequestTotalActiveAsync(AppId);
                _logger.LogInformation($"[Evacuation] Got total active from engine: {totalRequired}");
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("[Evacuation] Engine timeout for total active, using 0");
                // Security can update manually via UI
                totalRequired = 0;
            }

            // Update status
            alert.AlertStatus = EvacuationAlertStatus.Active;
            alert.StartedAt = DateTime.UtcNow;
            alert.TotalRequired = totalRequired;
            SetUpdateAudit(alert);
            await _repository.UpdateAsync(alert);

            // Publish to MQTT for engine
            var triggerDto = new EvacuationTriggerMqttDto
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = "Active",
                TriggerType = alert.TriggerType.ToString(),
                TriggeredAt = alert.StartedAt?.ToString("o"),
                ApplicationId = alert.ApplicationId.ToString()
            };

            await _evacuationMqtt.PublishEvacuationTriggerAsync(alert.ApplicationId, triggerDto);
            _logger.LogInformation($"[Evacuation] Started evacuation {alert.Id}, TotalRequired: {totalRequired}");

            _audit.Updated(
                "EvacuationAlert",
                alert.Id,
                "Started evacuation",
                new { alert.Title, alert.StartedAt, totalRequired }
            );

            return _mapper.Map<EvacuationAlertRead>(alert);
        }

        public async Task<EvacuationAlertRead> PauseAsync(Guid id)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            if (alert.AlertStatus != EvacuationAlertStatus.Active)
                throw new BusinessException("Can only pause active evacuations");

            alert.AlertStatus = EvacuationAlertStatus.Paused;
            SetUpdateAudit(alert);
            await _repository.UpdateAsync(alert);

            _audit.Updated(
                "EvacuationAlert",
                alert.Id,
                "Paused evacuation",
                new { alert.Title }
            );

            return _mapper.Map<EvacuationAlertRead>(alert);
        }

        public async Task<EvacuationAlertRead> CompleteAsync(Guid id, string? completionNotes)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            if (alert.AlertStatus != EvacuationAlertStatus.Active && alert.AlertStatus != EvacuationAlertStatus.Paused)
                throw new BusinessException("Can only complete active or paused evacuations");

            alert.AlertStatus = EvacuationAlertStatus.Completed;
            alert.CompletedAt = DateTime.UtcNow;
            alert.CompletionNotes = completionNotes;
            alert.CompletedBy = UsernameFormToken;
            SetUpdateAudit(alert);
            await _repository.UpdateAsync(alert);

            // Publish to MQTT for engine
            var completeDto = new EvacuationCompleteMqttDto
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = "Completed",
                CompletedAt = alert.CompletedAt?.ToString("o"),
                CompletedBy = alert.CompletedBy,
                CompletionNotes = completionNotes
            };

            await _evacuationMqtt.PublishEvacuationCompleteAsync(alert.ApplicationId, completeDto);
            _logger.LogInformation($"[Evacuation] Completed evacuation {alert.Id}, published to MQTT");

            _audit.Updated(
                "EvacuationAlert",
                alert.Id,
                "Completed evacuation",
                new { alert.Title, alert.CompletedAt, alert.CompletionNotes }
            );

            return _mapper.Map<EvacuationAlertRead>(alert);
        }

        public async Task<EvacuationAlertRead> CancelAsync(Guid id)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            if (alert.AlertStatus == EvacuationAlertStatus.Completed)
                throw new BusinessException("Cannot cancel completed evacuations");

            alert.AlertStatus = EvacuationAlertStatus.Cancelled;
            alert.CompletedAt = DateTime.UtcNow;
            SetUpdateAudit(alert);
            await _repository.UpdateAsync(alert);

            // Publish to MQTT for engine
            var cancelDto = new EvacuationCancelMqttDto
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = "Cancelled",
                CancelledAt = alert.CompletedAt?.ToString("o")
            };

            await _evacuationMqtt.PublishEvacuationCancelAsync(alert.ApplicationId, cancelDto);
            _logger.LogInformation($"[Evacuation] Cancelled evacuation {alert.Id}, published to MQTT");

            _audit.Updated(
                "EvacuationAlert",
                alert.Id,
                "Cancelled evacuation",
                new { alert.Title }
            );

            return _mapper.Map<EvacuationAlertRead>(alert);
        }

        public async Task<EvacuationSummaryDto> GetSummaryAsync(Guid id)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            // Get transactions grouped by assembly point
            var transactions = await _transactionRepository.GetByAlertIdEntitiesAsync(id);

            var byAssemblyPoint = transactions
                .GroupBy(t => t.EvacuationAssemblyPointId)
                .Select(g => new AssemblyPointSummaryDto
                {
                    AssemblyPointId = g.Key,
                    Name = g.First().EvacuationAssemblyPoint?.Name ?? "Unknown",
                    Evacuated = g.Count(t => t.PersonStatus == EvacuationPersonStatus.Evacuated || t.PersonStatus == EvacuationPersonStatus.Confirmed),
                    Confirmed = g.Count(t => t.PersonStatus == EvacuationPersonStatus.Confirmed)
                })
                .ToList();

            return new EvacuationSummaryDto
            {
                EvacuationAlertId = alert.Id,
                Title = alert.Title,
                AlertStatus = alert.AlertStatus,
                StartedAt = alert.StartedAt,
                TotalRequired = alert.TotalRequired,
                TotalEvacuated = alert.TotalEvacuated,
                TotalConfirmed = alert.TotalConfirmed,
                TotalSafe = alert.TotalSafe,
                TotalRemaining = alert.TotalRemaining,
                ByAssemblyPoint = byAssemblyPoint
            };
        }

        public async Task<List<EvacuationPersonStatusDto>> GetPersonStatusAsync(Guid id)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            var transactions = await _transactionRepository.GetByAlertIdEntitiesAsync(id);

            return transactions.Select(t => new EvacuationPersonStatusDto
            {
                TransactionId = t.Id,
                MemberId = t.MemberId,
                VisitorId = t.VisitorId,
                SecurityId = t.SecurityId,
                PersonName = t.Member?.Name ?? t.Visitor?.Name ?? t.Security?.Name,
                PersonIdentity = t.Member?.PersonId ?? t.Visitor?.PersonId ?? t.Security?.PersonId,
                PersonCategory = t.PersonCategory,
                PersonStatus = t.PersonStatus,
                AssemblyPointId = t.EvacuationAssemblyPointId,
                AssemblyPointName = t.EvacuationAssemblyPoint?.Name,
                DetectedAt = t.DetectedAt,
                ConfirmedAt = t.ConfirmedAt,
                ConfirmedBy = t.ConfirmedBy
            }).ToList();
        }

        private async Task UpdateAndPublishStatusAsync(Guid alertId)
        {
            var alert = await _repository.GetByIdEntityAsync(alertId);
            if (alert == null) return;

            // Publish status to frontend via MQTT
            var statusDto = new EvacuationStatusMqttDto
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = alert.AlertStatus.ToString(),
                Timestamp = DateTime.UtcNow.ToString("o"),
                TotalRequired = alert.TotalRequired,
                TotalEvacuated = alert.TotalEvacuated,
                TotalConfirmed = alert.TotalConfirmed,
                TotalRemaining = alert.TotalRemaining
            };

            await _evacuationMqtt.PublishEvacuationStatusAsync(alert.ApplicationId, statusDto);
        }

        public async Task UpdateAlertCountersAsync(Guid alertId,
            int totalRequired, int totalEvacuated, int totalConfirmed, int totalRemaining)
        {
            await _repository.UpdateCountersAsync(
                alertId, totalRequired, totalEvacuated, totalConfirmed, 0, totalRemaining);

            // Publish status to frontend
            await UpdateAndPublishStatusAsync(alertId);
        }
    }
}
