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
using BusinessLogic.Services.Background;
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
        private readonly IMqttPubQueue _mqttPubQueue;
        private readonly ILogger<EvacuationAlertService> _logger;

        public EvacuationAlertService(
            EvacuationAlertRepository repository,
            EvacuationTransactionRepository transactionRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            IMqttPubQueue mqttPubQueue,
            ILogger<EvacuationAlertService> logger) : base(httpContextAccessor)
        {
            _repository = repository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _audit = audit;
            _mqttPubQueue = mqttPubQueue;
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
                throw new BusinessException("Cannot create new evacuation while another is active");

            var alert = _mapper.Map<EvacuationAlert>(createDto);
            SetCreateAudit(alert);
            alert.Status = 1;
            alert.AlertStatus = EvacuationAlertStatus.Active; 
            alert.StartedAt = DateTime.UtcNow;
            alert.ApplicationId = AppId;
            alert.TriggeredBy = UsernameFormToken;

            await _repository.AddAsync(alert);

            // Publish trigger to MQTT immediately
            var triggerDto = new
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = "Active",
                TriggerType = alert.TriggerType.ToString(),
                TriggeredAt = alert.StartedAt?.ToString("o"),
                ApplicationId = alert.ApplicationId.ToString()
            };
            var topic = $"evacuation/trigger/{alert.ApplicationId}";
            var payload = JsonSerializer.Serialize(triggerDto);
            _mqttPubQueue.Enqueue(topic, payload);
            _logger.LogInformation($"[Evacuation] Created and started evacuation {alert.Id}, published to topic: {topic}");

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

            // Only allow update if active
            if (alert.AlertStatus != EvacuationAlertStatus.Active)
                throw new BusinessException("Can only update active evacuations");

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

            // Allow delete active evacuations (for cleanup)
            SetDeleteAudit(alert);
            alert.Status = 0;

            await _repository.DeleteAsync(id);

            _audit.Deleted(
                "EvacuationAlert",
                alert.Id,
                "Deleted evacuation alert",
                new { alert.Title, alert.AlertStatus }
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

        public async Task<EvacuationAlertRead> CompleteAsync(Guid id, string? completionNotes)
        {
            var alert = await _repository.GetByIdEntityAsync(id);
            if (alert == null)
                throw new NotFoundException("Evacuation Alert not found");

            if (alert.AlertStatus != EvacuationAlertStatus.Active)
                throw new BusinessException("Can only complete active evacuations");

            alert.AlertStatus = EvacuationAlertStatus.Completed;
            alert.CompletedAt = DateTime.UtcNow;
            alert.CompletionNotes = completionNotes;
            alert.CompletedBy = UsernameFormToken;
            SetUpdateAudit(alert);
            await _repository.UpdateAsync(alert);

            // Publish complete to MQTT for engine
            var completeDto = new
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = "Completed",
                CompletedAt = alert.CompletedAt?.ToString("o"),
                CompletedBy = alert.CompletedBy,
                CompletionNotes = completionNotes
            };
            var topic = $"evacuation/complete/{alert.ApplicationId}";
            var payload = JsonSerializer.Serialize(completeDto);
            _mqttPubQueue.Enqueue(topic, payload);
            _logger.LogInformation($"[Evacuation] Completed evacuation {alert.Id}, published to topic: {topic}");

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

            if (alert.AlertStatus == EvacuationAlertStatus.Cancelled)
                throw new BusinessException("Cannot cancel completed evacuations");

            // Cancel = Complete with cancellation notes
            alert.AlertStatus = EvacuationAlertStatus.Cancelled;
            alert.CompletedAt = DateTime.UtcNow;
            alert.CompletionNotes = "Cancelled by user";
            alert.CompletedBy = UsernameFormToken;
            SetUpdateAudit(alert);
            await _repository.UpdateAsync(alert);

            // Publish cancel to MQTT for engine
            var cancelDto = new
            {
                EvacuationAlertId = alert.Id.ToString(),
                Status = "Cancelled",
                CancelledAt = alert.CompletedAt?.ToString("o")
            };
            var topic = $"evacuation/cancel/{alert.ApplicationId}";
            var payload = JsonSerializer.Serialize(cancelDto);
            _mqttPubQueue.Enqueue(topic, payload);
            _logger.LogInformation($"[Evacuation] Cancelled evacuation {alert.Id}, published to topic: {topic}");

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
    }
}
