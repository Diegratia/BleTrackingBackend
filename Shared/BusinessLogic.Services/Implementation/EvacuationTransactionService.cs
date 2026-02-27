using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Background;
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
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;
        private readonly IMqttPubQueue _mqttPubQueue;
        private readonly ILogger<EvacuationTransactionService> _logger;

        public EvacuationTransactionService(
            EvacuationTransactionRepository repository,
            EvacuationAlertRepository alertRepository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            IMqttPubQueue mqttPubQueue,
            ILogger<EvacuationTransactionService> logger) : base(httpContextAccessor)
        {
            _repository = repository;
            _alertRepository = alertRepository;
            _mapper = mapper;
            _audit = audit;
            _mqttPubQueue = mqttPubQueue;
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

            var confirmMqttDto = new
            {
                TransactionId = transaction.Id.ToString(),
                EvacuationAlertId = transaction.EvacuationAlertId.ToString(),
                PersonStatus = ((int)confirmDto.PersonStatus).ToString(),
                ConfirmedAt = transaction.ConfirmedAt?.ToString("o"),
                ConfirmedBy = transaction.ConfirmedBy,
                MemberId = transaction.MemberId?.ToString(),
                VisitorId = transaction.VisitorId?.ToString(),
                SecurityId = transaction.SecurityId?.ToString(),
                PersonCategory = ((int)transaction.PersonCategory).ToString()
            };
            var topic = $"evacuation/person-confirm/{transaction.ApplicationId}";
            var payload = JsonSerializer.Serialize(confirmMqttDto);
            _mqttPubQueue.Enqueue(topic, payload);
            _logger.LogInformation($"[Evacuation] Confirmed transaction {transaction.Id}, published to topic: {topic}");

            _audit.Updated(
                "EvacuationTransaction",
                transaction.Id,
                $"Confirmed person status: {confirmDto.PersonStatus}"
            );
        }
    }
}
