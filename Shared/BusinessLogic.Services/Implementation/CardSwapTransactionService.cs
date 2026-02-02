using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using DataView;
using Entities.Models;
using Helpers.Consumer;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Repositories.Repository.RepoModel;
using Shared.Contracts;

namespace BusinessLogic.Services.Implementation
{
    public class CardSwapTransactionService : BaseService, ICardSwapTransactionService
    {
        private readonly CardSwapTransactionRepository _repo;
        private readonly CardRepository _cardRepo;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public CardSwapTransactionService(
            CardSwapTransactionRepository repo,
            CardRepository cardRepo,
            IMapper mapper,
            IAuditEmitter audit,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _repo = repo;
            _cardRepo = cardRepo;
            _mapper = mapper;
            _audit = audit;
        }

        // CRUD Operations
        public async Task<IEnumerable<CardSwapTransactionRead>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return data;
        }

        public async Task<CardSwapTransactionRead?> GetByIdAsync(Guid id)
        {
            var data = await _repo.GetByIdAsync(id);
            if (data == null)
                throw new NotFoundException($"CardSwapTransaction with id {id} not found");
            return data;
        }

        public async Task<CardSwapTransactionRead> CreateAsync(CardSwapTransactionCreateDto dto)
        {
            
            var entity = _mapper.Map<CardSwapTransaction>(dto);
            
            entity.Id = Guid.NewGuid();
            entity.ApplicationId = AppId;
            entity.SwapSequence = await _repo.GetNextSequenceAsync(dto.SwapChainId);
            entity.CardSwapStatus = CardSwapStatus.Active;
            entity.ExecutedAt = DateTime.UtcNow;
            
            
            await _repo.AddAsync(entity);
            
            await _audit.Created(
                "Card Swap Transaction",
                entity.Id,
                "Created swap transaction",
                new { entity.SwapType, entity.SwapChainId, entity.SwapSequence }
            );
            
            var result = await _repo.GetByIdAsync(entity.Id);
            return result ?? throw new Exception("Failed to reload after create");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, CardSwapTransactionFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "ExecutedAt";
            filter.SortDir = request.SortDir ?? "desc";
            filter.Search = request.SearchValue;
            
            var (data, total, filtered) = await _repo.FilterAsync(filter);
            
            return new
            {
                draw = request.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data
            };
        }

        // Business Logic: Get last active swap (LIFO)
        public async Task<CardSwapTransactionRead?> GetLastActiveSwapAsync(Guid visitorId, Guid swapChainId)
        {
            var entity = await _repo.GetLastActiveSwapAsync(visitorId, swapChainId);
            if (entity == null) return null;
            
            return _mapper.Map<CardSwapTransactionRead>(entity);
        }

        public async Task<CardSwapTransactionRead> PerformForwardSwapAsync(ForwardSwapRequest request)
        {
            var visitor = await _repo.GetVisitorByIdAsync(request.VisitorId);
            if (visitor == null)
                throw new NotFoundException($"Visitor with id {request.VisitorId} not found");
            if (string.IsNullOrEmpty(visitor.CardNumber))
                throw new BusinessException("Visitor does not have a registered card number");
            
            var area = await _repo.GetMaskedAreaByIdAsync(request.MaskedAreaId);
            if (area == null)
                throw new NotFoundException($"MaskedArea with id {request.MaskedAreaId} not found");
                    
            await ValidateCardAvailabilityAsync(request.ToCardId, "Tracking Card");

            // var visitorCard  = await _repo.GetCardbyCardNumber(visitor.CardNumber);
            //     if(visitorCard  == null)
            //         throw new NotFoundException($"Card with number {visitor.CardNumber} not found");
            
            // Get last active swap to determine FromCard
            var lastSwap = await _repo.GetLastActiveSwapAsync(request.VisitorId, Guid.Empty);
            // var fromCardId = lastSwap?.ToCardId ?? visitorCard .Id;
            
            var swapChainId = lastSwap?.SwapChainId ?? Guid.NewGuid();
            var swapBy = UsernameFormToken;
            var trxVisitor = await _repo.GetLatestTrxVisitor(visitor.Id);
                if(trxVisitor == null)
                    throw new NotFoundException($"TrxVisitor with visitor id {visitor.Id} not found");
                    
                Guid? fromCardId = null;
                Guid? toCardId = null;

                //DOMAIN DECISION BASED ON SwapMode
                switch (request.SwapMode)
                {
                    case SwapMode.CardSwap:
                        await ValidateCardAvailabilityAsync(request.ToCardId, "Tracking Card");
                        var visitorCard = await _repo.GetCardbyCardNumber(visitor.CardNumber!)
                            ?? throw new NotFoundException("Visitor card not found");

                        fromCardId = visitorCard.Id;
                        toCardId = request.ToCardId;
                        request.IdentityType = IdentityType.CardAccess;
                        break;

                    case SwapMode.HoldIdentity:
                        if (request.IdentityType == null || string.IsNullOrEmpty(request.IdentityValue))
                            throw new BusinessException("Identity is required for HoldIdentity");
                        break;

                    case SwapMode.CardAndIdentity:
                        await ValidateCardAvailabilityAsync(request.ToCardId, "Tracking Card");

                        var card = await _repo.GetCardbyCardNumber(visitor.CardNumber!)
                            ?? throw new NotFoundException("Visitor card not found");

                        fromCardId = card.Id;
                        toCardId = request.ToCardId;
                        break;

                    case SwapMode.ExtendAccess:
                        // hanya memperpanjang / menambah konteks akses
                        break;
                }

            var dto = new CardSwapTransactionCreateDto
            {
                VisitorId = request.VisitorId,
                TrxVisitorId = trxVisitor.Id,
                FromCardId = fromCardId,
                ToCardId = request.ToCardId,
                MaskedAreaId = request.MaskedAreaId,
                SwapType = SwapType.EnterArea,
                SwapMode = request.SwapMode,
                SwapChainId = swapChainId,
                IdentityType = request.IdentityType,
                IdentityValue = request.IdentityValue,
                SwapBy = swapBy
            
            };
            
            return await CreateAsync(dto);
        }

        // Business Logic: Reverse Swap (Exit Area - LIFO)
        public async Task<CardSwapTransactionRead> PerformReverseSwapAsync(ReverseSwapRequest request)
        {
            if (!await CanReverseSwapAsync(request.VisitorId, request.SwapChainId))
                throw new BusinessException("No active swap available for reverse operation");
            
            // Get last active swap (LIFO)
            var lastSwap = await _repo.GetLastActiveSwapAsync(request.VisitorId, request.SwapChainId);
            if (lastSwap == null)
                throw new NotFoundException("No active swap found for reverse");
            
            if (lastSwap.SwapType != SwapType.EnterArea)
                throw new BusinessException("Last swap is not an EnterArea swap");
            var swapBy = UsernameFormToken;
            
            var dto = new CardSwapTransactionCreateDto
            {
                VisitorId = request.VisitorId,
                TrxVisitorId = lastSwap.TrxVisitorId,
                FromCardId = lastSwap.ToCardId,  // Return the tracking card
                ToCardId = lastSwap.FromCardId,  // Give back the previous card
                MaskedAreaId = lastSwap.MaskedAreaId.Value,
                SwapType = SwapType.ExitArea,
                SwapChainId = request.SwapChainId,
                SwapBy = swapBy,
                IdentityType = null,
                IdentityValue = null
            };
            
            var result = await CreateAsync(dto);
            
            // Mark the previous swap as completed
            lastSwap.CardSwapStatus = CardSwapStatus.Completed;
            lastSwap.CompletedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(lastSwap);
            
            return result;
        }

        public async Task<bool> CanReverseSwapAsync(Guid visitorId, Guid swapChainId)
        {
            var lastSwap = await _repo.GetLastActiveSwapAsync(visitorId, swapChainId);
            return lastSwap != null && lastSwap.SwapType == SwapType.EnterArea;
        }

        // Get all active swaps for a chain
        public async Task<IEnumerable<CardSwapTransactionRead>> GetActiveSwapsByChainAsync(Guid swapChainId)
        {
            var entities = await _repo.GetActiveSwapsByChainAsync(swapChainId);
            return _mapper.Map<IEnumerable<CardSwapTransactionRead>>(entities);
        }

        // Private helper methods
        private async Task ValidateCardAvailabilityAsync(Guid cardId, string cardType)
        {
            var card = await _cardRepo.GetByIdAsync(cardId);
            if (card == null)
                throw new NotFoundException($"{cardType} with id {cardId} not found");
            
            if (card.CardStatus != CardStatus.Available)
                throw new BusinessException($"{cardType} is not available (status: {card.CardStatus})");
        }
    }
}
