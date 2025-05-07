using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Implementation
{
    public class TrackingTransactionService : ITrackingTransactionService
    {
        private readonly TrackingTransactionRepository _repository;
        private readonly IMapper _mapper;

        public TrackingTransactionService(TrackingTransactionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<TrackingTransactionDto> CreateTrackingTransactionAsync(TrackingTransactionCreateDto createDto)
        {
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            var transaction = _mapper.Map<TrackingTransaction>(createDto);
            transaction.Id = Guid.NewGuid();

            await _repository.AddAsync(transaction);
            return _mapper.Map<TrackingTransactionDto>(transaction);
        }

        public async Task<TrackingTransactionDto> GetTrackingTransactionByIdAsync(Guid id)
        {
            var transaction = await _repository.GetByIdWithIncludesAsync(id);
            return transaction == null ? null : _mapper.Map<TrackingTransactionDto>(transaction);
        }

        public async Task<IEnumerable<TrackingTransactionDto>> GetAllTrackingTransactionsAsync()
        {
            var transactions = await _repository.GetAllWithIncludesAsync();
            return _mapper.Map<IEnumerable<TrackingTransactionDto>>(transactions);
        }

        public async Task UpdateTrackingTransactionAsync(Guid id, TrackingTransactionUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"TrackingTransaction with ID {id} not found.");

            _mapper.Map(updateDto, transaction);
            await _repository.UpdateAsync(transaction);
        }

        public async Task DeleteTrackingTransactionAsync(Guid id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"TrackingTransaction with ID {id} not found.");

            await _repository.DeleteAsync(transaction);
        }
    }
}
