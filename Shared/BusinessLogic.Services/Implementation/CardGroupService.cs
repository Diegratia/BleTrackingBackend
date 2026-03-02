using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Implementation
{
    public class CardGroupService : BaseService, ICardGroupService
    {
        private readonly CardGroupRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public CardGroupService(
            CardGroupRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            CardRepository cardRepository,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<CardGroupRead> GetByIdAsync(Guid id)
        {
            var cardGroup = await _repository.GetByIdAsync(id);
            if (cardGroup == null)
                throw new NotFoundException($"Card Group with id {id} not found");
            return cardGroup;
        }

        public async Task<IEnumerable<CardGroupRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CardGroupRead> CreateAsync(CardGroupCreateDto dto)
        {
            var entity = _mapper.Map<CardGroup>(dto);
            entity.ApplicationId = AppId;
            SetCreateAudit(entity);
            entity.Status = 1;

            if (dto.CardIds.Any())
            {
                var cards = await _cardRepository.GetAllQueryable()
                    .Where(c => dto.CardIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var card in cards)
                {
                    if (card == null)
                        throw new NotFoundException($"Card with id {dto.CardIds.First()} not found");

                    card.CardGroupId = entity.Id;
                }
            }

            await _repository.AddAsync(entity);
            _audit.Created("CardGroup", entity.Id, $"CardGroup {entity.Name} created");

            return await _repository.GetByIdAsync(entity.Id);
        }

        public async Task UpdateAsync(Guid id, CardGroupUpdateDto dto)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new NotFoundException("Card Group not found");

            entity.Name = dto.Name ?? entity.Name;
            entity.Remarks = dto.Remarks ?? entity.Remarks;
            SetUpdateAudit(entity);

            var existingCards = await _cardRepository.GetAllQueryable()
                .Where(c => c.CardGroupId == entity.Id)
                .ToListAsync();

            foreach (var card in existingCards)
            {
                card.CardGroupId = null;
            }

            if (dto.CardIds.Any())
            {
                var cards = await _cardRepository.GetAllQueryable()
                    .Where(c => dto.CardIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var card in cards)
                {
                    if (card == null)
                        throw new NotFoundException($"Card with id {dto.CardIds.First()} not found");

                    card.CardGroupId = entity.Id;
                }
            }

            await _repository.UpdateAsync(entity);
            _audit.Updated("CardGroup", entity.Id, $"CardGroup {entity.Name} updated");
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new NotFoundException("Card Group not found");

            var existingCards = await _cardRepository.GetAllQueryable()
                .Where(c => c.CardGroupId == entity.Id)
                .ToListAsync();

            foreach (var card in existingCards)
            {
                card.CardGroupId = null;
            }

            SetDeleteAudit(entity);
            await _repository.DeleteAsync(entity);
            _audit.Deleted("CardGroup", id, $"CardGroup {entity.Name} deleted");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, CardGroupFilter filter)
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
                data
            };
        }
    }
}
