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
    public class EvacuationTransactionService : BaseService, IEvacuationTransactionService
    {
        private readonly EvacuationTransactionRepository _repository;
        private readonly EvacuationAlertRepository _alertRepository;
        private readonly IEvacuationAlertService _alertService;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;
        private readonly ILogger<EvacuationTransactionService> _logger;

        public EvacuationTransactionService(
            EvacuationTransactionRepository repository,
            EvacuationAlertRepository alertRepository,
            IEvacuationAlertService alertService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            ILogger<EvacuationTransactionService> logger) : base(httpContextAccessor)
        {
            _repository = repository;
            _alertRepository = alertRepository;
            _alertService = alertService;
            _mapper = mapper;
            _audit = audit;
            _logger = logger;
        }

        public async Task<EvacuationTransactionRead> GetByIdAsync(Guid id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
                throw new NotFoundException($"Evacuation Transaction with id {id} not found");
            return transaction;
        }

        public async Task<IEnumerable<EvacuationTransactionRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<EvacuationTransactionRead> GetByAlertIdAsync(Guid alertId)
        {
            var alert = await _alertRepository.GetByIdEntityAsync(alertId);
            if (alert == null)
                throw new NotFoundException($"Evacuation Alert with id {alertId} not found");

            var transactions = await _repository.GetByAlertIdAsync(alertId);
            return transactions.FirstOrDefault() ?? throw new NotFoundException("No transactions found");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, EvacuationTransactionFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "DetectedAt";
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

        public async Task ConfirmAsync(Guid transactionId, EvacuationTransactionConfirmDto confirmDto)
        {
            var transaction = await _repository.GetByIdEntityAsync(transactionId);
            if (transaction == null)
                throw new NotFoundException("Evacuation Transaction not found");

            // Update transaction
            transaction.PersonStatus = confirmDto.PersonStatus;
            transaction.ConfirmedBy = UsernameFormToken;
            transaction.ConfirmedAt = DateTime.UtcNow;
            transaction.ConfirmationNotes = confirmDto.ConfirmationNotes;
            SetUpdateAudit(transaction);

            await _repository.UpdateAsync(transaction);

            // Update alert counters
            await UpdateAlertCountersAsync(transaction.EvacuationAlertId);

            _audit.Updated(
                "EvacuationTransaction",
                transaction.Id,
                $"Confirmed person status: {confirmDto.PersonStatus}"
            );
        }

        public async Task ProcessDetectionAsync(EvacuationDetectionDto detectionDto)
        {
            var alert = await _alertRepository.GetByIdEntityAsync(detectionDto.EvacuationAlertId);
            if (alert == null)
            {
                _logger.LogWarning($"[Evacuation] Alert {detectionDto.EvacuationAlertId} not found");
                return;
            }

            if (alert.AlertStatus != EvacuationAlertStatus.Active)
            {
                _logger.LogWarning($"[Evacuation] Alert {detectionDto.EvacuationAlertId} is not active");
                return;
            }

            foreach (var person in detectionDto.Persons)
            {
                // Check if transaction already exists
                var existing = await _repository.GetByPersonAndAlertAsync(
                    detectionDto.EvacuationAlertId,
                    person.MemberId,
                    person.VisitorId,
                    person.SecurityId);

                if (existing != null)
                {
                    // Update existing transaction - only last detected time
                    existing.LastDetectedAt = DateTime.UtcNow;

                    // Only update status if still remaining
                    if (existing.PersonStatus == EvacuationPersonStatus.Remaining)
                    {
                        existing.PersonStatus = EvacuationPersonStatus.Evacuated;
                    }

                    SetUpdateAudit(existing);
                    await _repository.UpdateAsync(existing);
                }
                else
                {
                    // Create new transaction
                    var transaction = new EvacuationTransaction
                    {
                        Id = Guid.NewGuid(),
                        EvacuationAlertId = detectionDto.EvacuationAlertId,
                        EvacuationAssemblyPointId = detectionDto.AssemblyPointId,
                        PersonCategory = person.PersonCategory,
                        MemberId = person.MemberId,
                        VisitorId = person.VisitorId,
                        SecurityId = person.SecurityId,
                        CardId = person.CardId,
                        PersonStatus = EvacuationPersonStatus.Evacuated,
                        DetectedAt = detectionDto.DetectedAt,
                        ApplicationId = alert.ApplicationId
                    };

                    SetCreateAudit(transaction);
                    await _repository.AddAsync(transaction);
                }
            }

            // Update alert counters
            await UpdateAlertCountersAsync(detectionDto.EvacuationAlertId);

            _logger.LogInformation($"[Evacuation] Processed {detectionDto.Persons.Count} detections for alert {detectionDto.EvacuationAlertId}");
        }

        private async Task UpdateAlertCountersAsync(Guid alertId)
        {
            var transactions = await _repository.GetByAlertIdAsync(alertId);

            var totalRequired = transactions.Count;
            var totalEvacuated = transactions.Count(t => t.PersonStatus == EvacuationPersonStatus.Evacuated
                || t.PersonStatus == EvacuationPersonStatus.Confirmed);
            var totalConfirmed = transactions.Count(t => t.PersonStatus == EvacuationPersonStatus.Confirmed);
            var totalRemaining = totalRequired - totalEvacuated;

            await _alertRepository.UpdateCountersAsync(
                alertId,
                totalRequired,
                totalEvacuated,
                totalConfirmed,
                0,  // TotalSafe
                totalRemaining);
        }

        public async Task UpdateSummaryAsync(Guid alertId, EvacuationSummaryUpdateDto summaryDto)
        {
            // Update alert counters via AlertService (which also publishes status)
            await _alertService.UpdateAlertCountersAsync(
                alertId,
                summaryDto.TotalRequired,
                summaryDto.TotalEvacuated,
                summaryDto.TotalConfirmed,
                summaryDto.TotalRemaining);
        }
    }
}
