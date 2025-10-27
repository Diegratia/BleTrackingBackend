using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer.DtoHelpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation
{
    public class CardAccessService : ICardAccessService
    {
        private readonly CardAccessRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private FloorplanMaskedAreaRepository _areaRepository;
        private TimeGroupRepository _timeGroupRepository;
        private CardRepository _cardRepository;

        public CardAccessService(CardAccessRepository repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        FloorplanMaskedAreaRepository areaRepository,
        TimeGroupRepository timeGroupRepository,
        CardRepository cardRepository)
        {
            _repository = repository;
            _timeGroupRepository = timeGroupRepository;
            _areaRepository = areaRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cardRepository = cardRepository;
        }



        // public async Task<object> FilterAsync(DataTablesRequest request)
        // {
        //     var query = _repository.GetAllQueryable();

        //     var searchableColumns = new[] { "Name", "AccessNumber", "AccessScope" };
        //     var validSortColumns = new[] { "UpdatedAt", "Status" , "AccessNumber", "AccessScope" };

        //     var filterService = new GenericDataTableService<CardAccess, CardAccessDto>(
        //         query,
        //         _mapper,
        //         searchableColumns,
        //         validSortColumns);

        //     return await filterService.FilterAsync(request);
        // }
        

                public async Task<CardAccessDto> CreateAsync(CardAccessCreateDto dto)
        {
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = _mapper.Map<CardAccess>(dto);
            entity.Id = Guid.NewGuid();
            int? currentMax = _repository.GetAllQueryable().Max(m => m.AccessNumber);
            int increment = currentMax.HasValue ? currentMax.Value + 1 : 1;
            entity.AccessNumber = increment;
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = 1;

            if (applicationIdClaim != null)
                entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

            // 🔹 1️⃣ Resolve semua MaskedArea berdasarkan lokasi
            var resolvedMaskedAreaIds = await _areaRepository.GetMaskedAreaIdsByLocationAsync(
                dto.BuildingId, dto.FloorId, dto.FloorplanId
            );

            // Gabungkan dengan MaskedAreaIds manual dari input (kalau ada)
            if (dto.MaskedAreaIds?.Any() == true)
            {
                resolvedMaskedAreaIds.AddRange(dto.MaskedAreaIds.Where(x => x.HasValue).Select(x => x.Value));
            }

            resolvedMaskedAreaIds = resolvedMaskedAreaIds.Distinct().ToList();

            // 🔹 2️⃣ Tambahkan ke entitas
            if (resolvedMaskedAreaIds.Any())
            {
                entity.CardAccessMaskedAreas = resolvedMaskedAreaIds
                    .Select(id => new CardAccessMaskedArea
                    {
                        CardAccessId = entity.Id,
                        MaskedAreaId = id,
                        ApplicationId = entity.ApplicationId
                    }).ToList();
            }

            // 🔹 3️⃣ TimeGroups tetap seperti sebelumnya
            if (dto.TimeGroupIds.Any())
            {
                var validTimegroups = await _timeGroupRepository.GetAllQueryable()
                    .Where(t => dto.TimeGroupIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                entity.CardAccessTimeGroups = validTimegroups
                    .Select(id => new CardAccessTimeGroups
                    {
                        CardAccessId = entity.Id,
                        TimeGroupId = id,
                        ApplicationId = entity.ApplicationId
                    }).ToList();
            }

            await _repository.AddAsync(entity);

            var dtoResult = _mapper.Map<CardAccessDto>(entity);
            dtoResult.MaskedAreaIds = entity.CardAccessMaskedAreas?
                .Select(x => (Guid?)x.MaskedAreaId)
                .ToList();
            dtoResult.TimeGroupIds = entity.CardAccessTimeGroups?
                .Select(x => (Guid?)x.TimeGroupId)
                .ToList();

            return dtoResult;
        }


        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.MinimalGetAllQueryableDto();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Status", "AccessNumber", "AccessScope" };

            var filterService = new MinimalGenericDataTableService<CardAccessMinimalDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }

        public async Task UpdateAsync(Guid id, CardAccessUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
                throw new KeyNotFoundException("CardAccess not found");

            _mapper.Map(dto, entity);

            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

                    // update masked areas
        entity.CardAccessMaskedAreas.Clear();

        var resolvedMaskedAreaIds = await _areaRepository.GetMaskedAreaIdsByLocationAsync(
            dto.BuildingId, dto.FloorId, dto.FloorplanId
        );

        if (dto.MaskedAreaIds?.Any() == true)
        {
            resolvedMaskedAreaIds.AddRange(dto.MaskedAreaIds.Where(x => x.HasValue).Select(x => x.Value));
        }

        resolvedMaskedAreaIds = resolvedMaskedAreaIds.Distinct().ToList();

        if (resolvedMaskedAreaIds.Any())
        {
            entity.CardAccessMaskedAreas = resolvedMaskedAreaIds
                .Select(id => new CardAccessMaskedArea
                {
                    CardAccessId = entity.Id,
                    MaskedAreaId = id,
                    ApplicationId = entity.ApplicationId
                }).ToList();
        }

            await _repository.UpdateAsync(entity);
        }

        // public async Task<IEnumerable<CardAccessDto>> GetAllAsync()
        // {
        //     var list = await _repository.GetAllAsync();

        //     return _mapper.Map<IEnumerable<CardAccessDto>>(list);

        // }

        public async Task<IEnumerable<CardAccessDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<CardAccessDto>>(entities);

            foreach (var dto in dtos)
            {
                var entity = entities.First(e => e.Id == dto.Id);
                dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                    .Select(x => (Guid?)x.MaskedAreaId)
                    .ToList();
                dto.TimeGroupIds = entity.CardAccessTimeGroups
                    .Select(x => (Guid?)x.TimeGroupId)
                    .ToList();
            }

            return dtos;
        }


        public async Task<CardAccessDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            var dto = _mapper.Map<CardAccessDto>(entity);
            dto.MaskedAreaIds = entity.CardAccessMaskedAreas
                .Select(x => (Guid?)x.MaskedAreaId)
                .ToList();
            dto.TimeGroupIds = entity.CardAccessTimeGroups
                    .Select(x => (Guid?)x.TimeGroupId)
                    .ToList();

            return dto;
        }


        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Card Access not found");

            entity.Status = 0;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
        }

        // public async Task AssignCardAccessToCardAsync(CardAssignAccessDto dto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

        //     var card = await _context.Cards
        //         .Include(c => c.CardCardAccesses)
        //         .FirstOrDefaultAsync(c => c.Id == dto.CardId);

        //     if (card == null)
        //         throw new KeyNotFoundException("Card not found");

        //     // Bersihkan relasi lama
        //     card.CardCardAccesses.Clear();

        //     // Tambahkan relasi baru
        //     foreach (var accessId in dto.CardAccessIds.Distinct())
        //     {
        //         card.CardCardAccesses.Add(new CardCardAccess
        //         {
        //             CardId = card.Id,
        //             CardAccessId = accessId,
        //             ApplicationId = card.ApplicationId,
        //             Status = 1
        //         });
        //     }

        //     await _context.SaveChangesAsync();
        // }
        

            public async Task AssignCardAccessToCardAsync(CardAssignAccessDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            // 1️⃣ Ambil semua accessId yang relevan
            var allAccessIds = new HashSet<Guid>();

            // Manual CardAccessIds dari input
            if (dto.CardAccessIds?.Any() == true)
            {
                foreach (var id in dto.CardAccessIds)
                    allAccessIds.Add(id);
            }

            // Dari lokasi (building/floor/floorplan)
            var locationAccessIds = await _repository.GetCardAccessIdsByLocationAsync(
                dto.BuildingId, dto.FloorId, dto.FloorplanId);

            foreach (var id in locationAccessIds)
                allAccessIds.Add(id);

            // 2️⃣ Simpan lewat CardRepository
            await _cardRepository.AssignCardAccessAsync(dto.CardId, allAccessIds, username);
        }

    }
}



// public async Task<CardAccessDto> CreateAsync(CardAccessCreateDto dto)
//         {
//             var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

//             var entity = _mapper.Map<CardAccess>(dto);
//             entity.Id = Guid.NewGuid();
//             int? currentMax = _repository.GetAllQueryable().Max(m => m.AccessNumber);
//             int increment = currentMax.HasValue ? currentMax.Value + 1 : 1;
//             entity.AccessNumber = increment;
//             entity.CreatedBy = username;
//             entity.CreatedAt = DateTime.UtcNow;
//             entity.UpdatedBy = username;
//             entity.UpdatedAt = DateTime.UtcNow;
//             entity.Status = 1;

//             if (applicationIdClaim != null)
//                 entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

//             // MaskedAreas
//             if (dto.MaskedAreaIds.Any())
//             {
//                 var validMaskedAreas = await _areaRepository.GetAllQueryable()
//                     .Where(m => dto.MaskedAreaIds.Contains(m.Id))
//                     .Select(m => m.Id)
//                     .ToListAsync();

//                 if (validMaskedAreas.Count != dto.MaskedAreaIds.Count)
//                     throw new ArgumentException("Some MaskedAreaIds are invalid.");

//                 entity.CardAccessMaskedAreas = validMaskedAreas
//                     .Select(id => new CardAccessMaskedArea
//                     {
//                         CardAccessId = entity.Id,
//                         MaskedAreaId = id,
//                         ApplicationId = entity.ApplicationId
//                     }).ToList();
//             }

//             // TimeGroups
//             if (dto.TimeGroupIds.Any())
//             {
//                 var validTimegroups = await _timeGroupRepository.GetAllQueryable()
//                     .Where(t => dto.TimeGroupIds.Contains(t.Id))
//                     .Select(t => t.Id)
//                     .ToListAsync();

//                 if (validTimegroups.Count != dto.TimeGroupIds.Count)
//                     throw new ArgumentException("Some TimeGroupIds are invalid.");

//                 entity.CardAccessTimeGroups = validTimegroups
//                     .Select(id => new CardAccessTimeGroups
//                     {
//                         CardAccessId = entity.Id,
//                         TimeGroupId = id,
//                         ApplicationId = entity.ApplicationId
//                     }).ToList();
//             }


//             await _repository.AddAsync(entity);

//             var dtoResult = _mapper.Map<CardAccessDto>(entity);
//             dtoResult.MaskedAreaIds = entity.CardAccessMaskedAreas?
//                 .Select(x => (Guid?)x.MaskedAreaId)
//                 .ToList();
//             dtoResult.TimeGroupIds = entity.CardAccessTimeGroups?
//                 .Select(x => (Guid?)x.TimeGroupId)
//                 .ToList();

//             return dtoResult;
//         }





// public async Task UpdateAsync(Guid id, CardAccessUpdateDto dto)
//         {
//             var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
//             var entity = await _repository.GetByIdAsync(id);

//             if (entity == null)
//                 throw new KeyNotFoundException("CardAccess not found");

//             _mapper.Map(dto, entity);

//             entity.UpdatedBy = username;
//             entity.UpdatedAt = DateTime.UtcNow;

//             // update masked areas
//             entity.CardAccessMaskedAreas.Clear();
//             if (dto.MaskedAreaIds.Any())
//             {
//                 entity.CardAccessMaskedAreas = dto.MaskedAreaIds
//                     .Where(id => id.HasValue)
//                     .Select(id => new CardAccessMaskedArea
//                     {
//                         CardAccessId = entity.Id,
//                         MaskedAreaId = id.Value,
//                         ApplicationId = entity.ApplicationId
//                     })
//                     .ToList();
//             }

//             entity.CardAccessTimeGroups.Clear();
//             // TimeGroups
//             if (dto.TimeGroupIds.Any())
//             {
//                 entity.CardAccessTimeGroups = dto.TimeGroupIds
//                     .Where(id => id.HasValue)
//                     .Select(id => new CardAccessTimeGroups
//                     {
//                         CardAccessId = entity.Id,
//                         TimeGroupId = id.Value,
//                         ApplicationId = entity.ApplicationId
//                     })
//                     .ToList();
//             }

//             await _repository.UpdateAsync(entity);
//         }