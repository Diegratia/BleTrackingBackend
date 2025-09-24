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
using Helpers.Consumer;

namespace BusinessLogic.Services.Implementation
{
    public class CardGroupService : ICardGroupService
    {
        private readonly CardGroupRepository _repository;
        private readonly CardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CardGroupService(CardGroupRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, CardRepository cardRepository)
        {
            _repository = repository;
            _cardRepository = cardRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        // public async Task<CardGroupDto> CreateAsync(CardGroupCreateDto dto)
        // {
        //     var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
        //     var entity = _mapper.Map<CardGroup>(dto);

        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     entity.Id = Guid.NewGuid();
        //     entity.CreatedBy = username;
        //     entity.CreatedAt = DateTime.UtcNow;
        //     entity.UpdatedBy = username;
        //     entity.UpdatedAt = DateTime.UtcNow;
        //     entity.Status = 1;

        //     if (applicationIdClaim != null)
        //         entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

        // if (dto.CardIds.Any())
        //     {
        //         var cards = await _cardRepository.GetAllQueryable()
        //                         .Where(c => dto.CardIds.Contains(c.Id))
        //                         .ToListAsync();

        //         foreach (var card in cards)
        //         {
        //             if (card == null)
        //             {
        //                 throw new KeyNotFoundException($"Card Access with id {dto.CardIds.First()} not found");
        //             }

        //             card.UpdatedAt = DateTime.UtcNow;
        //             card.UpdatedBy = username;
        //             card.ApplicationId = entity.ApplicationId;
        //             card.CardGroupId = entity.Id;
        //         }
        //     }

        // if (dto.CardAccessIds.Any())
        //     {
        //         var accesses = await _cardAccessRepository.GetAllQueryable()
        //                             .Where(c => dto.CardAccessIds.Contains(c.Id))
        //                             .ToListAsync();

        //         foreach (var access in accesses)
        //         {
        //             if (access == null)
        //             {
        //                 throw new KeyNotFoundException($"Card Access with id {dto.CardAccessIds.First()} not found");
        //             }
        //             access.UpdatedAt = DateTime.UtcNow;
        //             access.UpdatedBy = username;
        //             access.ApplicationId = entity.ApplicationId;

        //             // 🔹 Tambahkan baris di join table
        //             entity.CardGroupCardAccesses.Add(new CardGroupCardAccess
        //             {
        //                 CardGroupId = entity.Id,
        //                 CardAccessId = access.Id,
        //                 ApplicationId = entity.ApplicationId
        //             });
        //         }
        //     }
        //         var result = await _repository.AddAsync(entity);
        //         return _mapper.Map<CardGroupDto>(result);  
        // }

        public async Task<CardGroupDto> CreateAsync(CardGroupCreateDto dto)
        {
            var entity = _mapper.Map<CardGroup>(dto);

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            entity.Id = Guid.NewGuid();
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = 1;

            if (dto.CardIds.Any())
            {
                var cards = await _cardRepository.GetAllQueryable()
                                .Where(c => dto.CardIds.Contains(c.Id))
                                .ToListAsync();

                foreach (var card in cards)
                {
                    if (card == null)
                    {
                        throw new KeyNotFoundException($"Card Access with id {dto.CardIds.First()} not found");
                    }

                    card.UpdatedAt = DateTime.UtcNow;
                    card.UpdatedBy = username;
                    card.ApplicationId = entity.ApplicationId;
                    card.CardGroupId = entity.Id;
                }
            }
                var result = await _repository.AddAsync(entity);
                return _mapper.Map<CardGroupDto>(result);  
        }



        public async Task UpdateAsync(Guid id, CardGroupUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = await _repository.GetAllQueryable()
                .Include(cg => cg.Cards)
                // .Include(cg => cg.CardGroupCardAccesses)
                .FirstOrDefaultAsync(cg => cg.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("Card Group not found");

            // Update scalar
            entity.Name = dto.Name ?? entity.Name;
            entity.Remarks = dto.Remarks ?? entity.Remarks;
            // entity.AccessScope = Enum.Parse<AccessScope>(dto.AccessScope ?? entity.AccessScope.ToString());
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            // =============================
            // 🔹 Update Cards
            // =============================

            // Ambil semua card yang sekarang terikat ke group ini
            var existingCards = await _cardRepository.GetAllQueryable()
                .Where(c => c.CardGroupId == entity.Id)
                .ToListAsync();

            // Kosongkan hubungan lama (set null)
            foreach (var card in existingCards)
            {
                card.CardGroupId = null;
                card.UpdatedAt = DateTime.UtcNow;
                card.UpdatedBy = username;
            }

            // Kalau ada CardIds di request, assign ulang
            if (dto.CardIds.Any())
            {
                var cards = await _cardRepository.GetAllQueryable()
                                    .Where(c => dto.CardIds.Contains(c.Id))
                                    .ToListAsync();

                foreach (var card in cards)
                {
                    if (card == null)
                        throw new KeyNotFoundException($"Card with id {dto.CardIds.First()} not found");

                    card.CardGroupId = entity.Id;
                    card.UpdatedAt = DateTime.UtcNow;
                    card.UpdatedBy = username;
                }
            }

            // =============================
            // 🔹 Update CardAccesses (join table)
            // =============================
            // var existingAccessIds = entity.CardGroupCardAccesses.Select(ca => ca.CardAccessId).ToList();
            // var newAccessIds = dto.CardAccessIds.Where(id => id.HasValue).Select(id => id.Value).ToList();

            // // Remove yang tidak ada di request
            // var toRemove = entity.CardGroupCardAccesses
            //     .Where(ca => !newAccessIds.Contains(ca.CardAccessId))
            //     .ToList();

            // foreach (var remove in toRemove)
            // {
            //     entity.CardGroupCardAccesses.Remove(remove);
            // }

            // // Tambah yang baru
            // var toAdd = newAccessIds.Except(existingAccessIds).ToList();
            // foreach (var addId in toAdd)
            // {
            //     var access = await _cardAccessRepository.GetByIdAsync(addId);
            //     if (access == null)
            //         throw new KeyNotFoundException($"Card Access with id {addId} not found");

            //     entity.CardGroupCardAccesses.Add(new CardGroupCardAccess
            //     {
            //         CardGroupId = entity.Id,
            //         CardAccessId = addId,
            //         ApplicationId = entity.ApplicationId
            //     });
            // }

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

            var searchableColumns = new[] { "Name", "Card", "AccessScope" };
            var validSortColumns = new[] { "UpdatedAt", "Name", "Status" };

            var filterService = new GenericDataTableService<CardGroup, CardGroupDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

    }
}