using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Repositories.Repository;
using Microsoft.AspNetCore.Http;
using Entities.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Implementation
{
    public class CardGroupService : ICardGroupService
    {
        private readonly CardGroupRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly CardAccessRepository _cardAccessRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardGroupService(CardGroupRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, CardRepository cardRepository, CardAccessRepository cardAccessRepository)
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _cardAccessRepository = cardAccessRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CardGroupDto> CreateAsync(CardGroupCreateDto dto)
        {
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
            var entity = _mapper.Map<CardGroup>(dto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            entity.Id = Guid.NewGuid();
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = 1;

            if (applicationIdClaim != null)
                entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

           if (dto.CardIds.Any())
            {
                var cards = await _cardRepository.GetAllQueryable()
                                .Where(c => dto.CardIds.Contains(c.Id))
                                .ToListAsync();

                foreach (var card in cards)
                {
                    card.UpdatedAt = DateTime.UtcNow;
                    card.UpdatedBy = username;
                    card.ApplicationId = entity.ApplicationId;
                    card.CardGroupId = entity.Id;
                }
            }

           if (dto.CardAccessIds.Any())
            {
                var accesses = await _cardAccessRepository.GetAllQueryable()
                                    .Where(c => dto.CardAccessIds.Contains(c.Id))
                                    .ToListAsync();

                foreach (var access in accesses)
                {
                    access.UpdatedAt = DateTime.UtcNow;
                    access.UpdatedBy = username;
                    access.ApplicationId = entity.ApplicationId;

                    // 🔹 Tambahkan baris di join table
                    entity.CardGroupCardAccesses.Add(new CardGroupCardAccess
                    {
                        CardGroupId = entity.Id,
                        CardAccessId = access.Id,
                        ApplicationId = entity.ApplicationId
                    });
                }
            }


                var result = await _repository.AddAsync(entity);
                return _mapper.Map<CardGroupDto>(result);  
        }



        public async Task UpdateAsync(Guid id, CardGroupUpdateDto dto)
        {
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
            var entity = _mapper.Map<CardGroup>(dto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            if (applicationIdClaim != null)
                entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

            if (dto.CardIds.Any())
            {
                var cards = await _cardRepository.GetAllQueryable()
                                .Where(c => dto.CardIds.Contains(c.Id))
                                .ToListAsync();

                foreach (var card in cards)
                {
                    card.UpdatedAt = DateTime.UtcNow;
                    card.UpdatedBy = username;
                    card.ApplicationId = entity.ApplicationId;
                    card.CardGroupId = entity.Id;
                }
            }

            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Card Group not found");

            entity.Status = 0;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CardGroupDto>> GetAllAsync()
        {
            var cardGroups = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CardGroupDto>>(cardGroups);
        }


        public async Task<CardGroupDto> GetByIdAsync(Guid id)
        {
            var cardGroups = await _repository.GetByIdAsync(id);
            return cardGroups == null ? null : _mapper.Map<CardGroupDto>(cardGroups);
        }
        
        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name", "Card" };
            var validSortColumns = new[] { "Name", "UpdatedAt", "Status" };

            var filterService = new GenericDataTableService<CardGroup, CardGroupDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

    }
}