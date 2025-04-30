using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class TrackingTransactionService : ITrackingTransactionService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public TrackingTransactionService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TrackingTransactionDto> CreateTrackingTransactionAsync(TrackingTransactionCreateDto createDto)
        {
            if (createDto == null) throw new ArgumentNullException(nameof(createDto));

            var transaction = _mapper.Map<TrackingTransaction>(createDto);
            transaction.Id = Guid.NewGuid();

            _context.TrackingTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return _mapper.Map<TrackingTransactionDto>(transaction);
        }

        public async Task<TrackingTransactionDto> GetTrackingTransactionByIdAsync(Guid id)
        {
            var transaction = await _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .FirstOrDefaultAsync(t => t.Id == id);
            return _mapper.Map<TrackingTransactionDto>(transaction);
        }

        public async Task<IEnumerable<TrackingTransactionDto>> GetAllTrackingTransactionsAsync()
        {
            var transactions = await _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .ToListAsync();
            return _mapper.Map<IEnumerable<TrackingTransactionDto>>(transactions);
        }

        public async Task UpdateTrackingTransactionAsync(Guid id, TrackingTransactionUpdateDto updateDto)
        {
            if (updateDto == null) throw new ArgumentNullException(nameof(updateDto));

            var transaction = await _context.TrackingTransactions.FindAsync(id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"TrackingTransaction with ID {id} not found.");
            }

            _mapper.Map(updateDto, transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTrackingTransactionAsync(Guid id)
        {
            var transaction = await _context.TrackingTransactions.FindAsync(id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"TrackingTransaction with ID {id} not found.");
            }

            _context.TrackingTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}