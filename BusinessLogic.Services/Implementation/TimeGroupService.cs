using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
using Helpers.Consumer.DtoHelpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class TimeGroupService : ITimeGroupService
    {
        private readonly TimeGroupRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CardAccessRepository _cardAccessRepository;

        public TimeGroupService(
            TimeGroupRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            CardAccessRepository cardAccessRepository
        )
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cardAccessRepository = cardAccessRepository;
        }

        public async Task<IEnumerable<TimeGroupDto>> GetAllsAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<TimeGroupDto>>(entities);

             foreach (var dto in dtos)
            {
                var entity = entities.First(e => e.Id == dto.Id);
                dto.CardAccessIds = entity.CardAccessTimeGroups
                    .Select(x => (Guid?)x.CardAccessId)
                    .ToList();
            }

            return dtos;
        }

        public async Task<TimeGroupDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;
            var dto = _mapper.Map<TimeGroupDto>(entity);
            dto.CardAccessIds = entity.CardAccessTimeGroups
                .Select(x => (Guid?)x.CardAccessId)
                .ToList();

            return dto;
        }
        

        // public async Task<TimeGroupDto> CreateAsync(TimeGroupCreateDto dto)
        // {
        //     var entity = _mapper.Map<TimeGroup>(dto);
        //     var result = await _repository.AddAsync(entity);
        //     return _mapper.Map<TimeGroupDto>(result);
        // }

        public async Task<TimeGroupDto> CreateAsync(TimeGroupCreateDto dto)
        {
            var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
            var entity = _mapper.Map<TimeGroup>(dto);
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            entity.Id = Guid.NewGuid();
            entity.ApplicationId = applicationIdClaim != null
                ? Guid.Parse(applicationIdClaim.Value)
                : Guid.Empty; // atau throw error kalau wajib

            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = 1;

            // Inject CardAccesses (optional)
            if (dto.CardAccessIds.Any())
            {
                var validAccesses = await _cardAccessRepository.GetAllQueryable()
                    .Where(ca => dto.CardAccessIds.Contains(ca.Id))
                    .Select(ca => ca.Id)
                    .ToListAsync();

                if (validAccesses.Count != dto.CardAccessIds.Count)
                    throw new ArgumentException("Some CardAccessIds are invalid.");
                if (applicationIdClaim != null)
                    entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

                entity.CardAccessTimeGroups = validAccesses
                    .Select(id => new CardAccessTimeGroups
                    {
                        TimeGroupId = entity.Id,
                        CardAccessId = id,
                        ApplicationId = entity.ApplicationId
                    })
                    .ToList();
            }

            // Inject ApplicationId ke semua time blocks
            if (applicationIdClaim != null)
            {
                foreach (var block in entity.TimeBlocks)
                {
                    block.ApplicationId = Guid.Parse(applicationIdClaim.Value);
                    block.Status = 1;
                    block.TimeGroupId = entity.Id;
                    block.CreatedBy = username;
                    block.UpdatedBy = username;
                    block.CreatedAt = DateTime.UtcNow;
                    block.UpdatedAt = DateTime.UtcNow;
                }
            }

            var result = await _repository.AddAsync(entity);

            var dtoResult = _mapper.Map<TimeGroupDto>(result);
            dtoResult.CardAccessIds = entity.CardAccessTimeGroups?
                .Select(x => (Guid?)x.CardAccessId)
                .ToList();

            return dtoResult;
        }

        //  public async Task UpdateAsync(Guid id, TimeGroupUpdateDto dto)
        // {
        //     var applicationIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

        //     var entity = await _repository.GetByIdAsync(id);
        //     if (entity == null)
        //         throw new KeyNotFoundException();

        //     // map property scalar saja (jangan map koleksi TimeBlocks)
        //     entity.Name = dto.Name;
        //     entity.Description = dto.Description;
        //     entity.UpdatedBy = username;
        //     entity.UpdatedAt = DateTime.UtcNow;

        //     if (applicationIdClaim != null)
        //         entity.ApplicationId = Guid.Parse(applicationIdClaim.Value);

        //     // update existing blocks
        //     foreach (var blockDto in dto.TimeBlocks)
        //     {
        //         var existing = entity.TimeBlocks.FirstOrDefault(b => b.Id == blockDto.Id);
        //         if (existing != null)
        //         {
        //             existing.DayOfWeek = !string.IsNullOrEmpty(blockDto.DayOfWeek)
        //                 ? Enum.Parse<DayOfWeek>(blockDto.DayOfWeek, true)
        //                 : null;
        //             existing.StartTime = blockDto.StartTime;
        //             existing.EndTime = blockDto.EndTime;
        //             existing.UpdatedAt = DateTime.UtcNow;
        //             existing.UpdatedBy = username;
        //         }
        //        else
        //         {
        //             var newBlock = new TimeBlock
        //             {
        //                 Id = Guid.NewGuid(), // di sini baru assign
        //                 DayOfWeek = !string.IsNullOrEmpty(blockDto.DayOfWeek)
        //                     ? Enum.Parse<DayOfWeek>(blockDto.DayOfWeek, true)
        //                     : null,
        //                 StartTime = blockDto.StartTime,
        //                 EndTime = blockDto.EndTime,
        //                 TimeGroupId = entity.Id,
        //                 ApplicationId = entity.ApplicationId,
        //                 Status = 1,
        //                 CreatedBy = username,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedBy = username,
        //                 UpdatedAt = DateTime.UtcNow
        //             };

        //             entity.TimeBlocks.Add(newBlock);
        //         }

        //     }

        //     entity.CardAccessTimeGroups.Clear();
        //      // TimeGroups
        //     if (dto.CardAccessIds.Any())
        //     {
        //         entity.CardAccessTimeGroups = dto.CardAccessIds
        //             .Where(id => id.HasValue)
        //             .Select(id => new CardAccessTimeGroups
        //             {
        //                 TimeGroupId = entity.Id,
        //                 CardAccessId = id.Value,
        //                 ApplicationId = entity.ApplicationId
        //             })
        //             .ToList();
        //     }

        //             Console.WriteLine("Blocks in entity: " + entity.TimeBlocks.Count);
        // foreach (var b in entity.TimeBlocks)
        // {
        //     Console.WriteLine($"Block: {b.Id}, Day={b.DayOfWeek}, Start={b.StartTime}, End={b.EndTime}");
        // }

        //     await _repository.UpdateAsync(entity);
        // }

        // public async Task UpdateAsync(Guid id, TimeGroupUpdateDto dto)
        // {
        //     var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
        //     var appIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");

        //     // ambil entity tracked via repo
        //     var entity = await _repository.GetByIdAsync(id);
        //     if (entity == null)
        //         throw new KeyNotFoundException("TimeGroup not found");

        //     // update scalar
        //     entity.Name = dto.Name ?? entity.Name;
        //     entity.Description = dto.Description ?? entity.Description;
        //     entity.UpdatedBy = username;
        //     entity.UpdatedAt = DateTime.UtcNow;

        //     if (appIdClaim != null)
        //         entity.ApplicationId = Guid.Parse(appIdClaim.Value);
        //     if (entity.ApplicationId == Guid.Empty)
        //         throw new ArgumentException("ApplicationId cannot be empty.");

        //     // sync timeblocks
        //     var dtoIds = dto.TimeBlocks
        //         .Where(b => b.Id.HasValue && b.Id.Value != Guid.Empty)
        //         .Select(b => b.Id!.Value)
        //         .ToList();

        //     foreach (var blockDto in dto.TimeBlocks)
        //     {
        //         if (blockDto.Id.HasValue && blockDto.Id.Value != Guid.Empty)
        //         {
        //             // update existing
        //             var existing = entity.TimeBlocks.FirstOrDefault(b => b.Id == blockDto.Id.Value);
        //             if (existing != null)
        //             {
        //                 existing.DayOfWeek = Enum.Parse<DayOfWeek>(blockDto.DayOfWeek, true);
        //                 existing.StartTime = blockDto.StartTime;
        //                 existing.EndTime = blockDto.EndTime;
        //                 existing.UpdatedAt = DateTime.UtcNow;
        //                 existing.UpdatedBy = username;
        //             }
        //         }
        //         else
        //         {
        //             // add new ✅ tracked otomatis karena parent tracked
        //             entity.TimeBlocks.Add(new TimeBlock
        //             {
        //                 Id = Guid.NewGuid(),
        //                 DayOfWeek = Enum.Parse<DayOfWeek>(blockDto.DayOfWeek, true),
        //                 StartTime = blockDto.StartTime,
        //                 EndTime = blockDto.EndTime,
        //                 TimeGroupId = entity.Id,
        //                 ApplicationId = entity.ApplicationId,
        //                 Status = 1,
        //                 CreatedBy = username,
        //                 CreatedAt = DateTime.UtcNow,
        //                 UpdatedBy = username,
        //                 UpdatedAt = DateTime.UtcNow
        //             });
        //         }
        //     }

        //     // remove lama
        //     var toRemove = entity.TimeBlocks
        //         .Where(b => !dtoIds.Contains(b.Id))
        //         .ToList();
        //     foreach (var remove in toRemove)
        //     {
        //         entity.TimeBlocks.Remove(remove);
        //     }

        //     // sync cardAccess
        //     if (dto.CardAccessIds != null)
        //     {
        //         var newIds = dto.CardAccessIds.Where(x => x.HasValue).Select(x => x.Value).ToList();
        //         var existingIds = entity.CardAccessTimeGroups.Select(c => c.CardAccessId).ToList();

        //         // remove
        //         var toRemoveAccess = entity.CardAccessTimeGroups
        //             .Where(c => !newIds.Contains(c.CardAccessId))
        //             .ToList();
        //         foreach (var rem in toRemoveAccess)
        //             entity.CardAccessTimeGroups.Remove(rem);

        //         // add
        //         var toAddAccess = newIds.Except(existingIds).ToList();
        //         foreach (var add in toAddAccess)
        //         {
        //             entity.CardAccessTimeGroups.Add(new CardAccessTimeGroups
        //             {
        //                 TimeGroupId = entity.Id,
        //                 CardAccessId = add,
        //                 ApplicationId = entity.ApplicationId
        //             });
        //         }
        //     }

        //     // commit via repo
        //     await _repository.UpdateAsync(entity);
        // }

        public async Task UpdateAsync(Guid id, TimeGroupUpdateDto dto)
{
    var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
    var appIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("ApplicationId");

    // 1. Load tracked entity (WITH TimeBlocks)
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null)
        throw new KeyNotFoundException("TimeGroup not found");

    // 2. Update scalar fields
    entity.Name = dto.Name;
    entity.Description = dto.Description;
    entity.UpdatedAt = DateTime.UtcNow;
    entity.UpdatedBy = username;

    if (appIdClaim != null)
        entity.ApplicationId = Guid.Parse(appIdClaim.Value);

    // ============================================================
    // 3. DELETE ALL OLD TIMEBLOCK (remove from ChangeTracker)
    // ============================================================
    foreach (var oldBlock in entity.TimeBlocks.ToList())
    {
        _repository.RemoveTimeBlock(oldBlock);   // state = Deleted
    }

    // Now clear navigation
    entity.TimeBlocks.Clear();

    // ============================================================
    // 4. ADD NEW TIMEBLOCKS
    // ============================================================
    foreach (var tb in dto.TimeBlocks)
    {
        var newBlock = new TimeBlock
        {
            Id = Guid.NewGuid(),
            DayOfWeek = !string.IsNullOrEmpty(tb.DayOfWeek)
                ? Enum.Parse<DayOfWeek>(tb.DayOfWeek, true)
                : (DayOfWeek?)null,
            StartTime = tb.StartTime,
            EndTime = tb.EndTime,
            Status = 1,
            ApplicationId = entity.ApplicationId,
            TimeGroupId = entity.Id,
            CreatedBy = username,
            CreatedAt = DateTime.UtcNow,
            UpdatedBy = username,
            UpdatedAt = DateTime.UtcNow
        };

        // Make EF recognize as Added
        _repository.AddTimeBlock(newBlock);

        // Add to navigation so FK relationship stays intact
        entity.TimeBlocks.Add(newBlock);
    }

    // ============================================================
    // 5. CARD ACCESS — replace
    // ============================================================
    entity.CardAccessTimeGroups.Clear();

    if (dto.CardAccessIds != null)
    {
        foreach (var idAccess in dto.CardAccessIds.Where(x => x.HasValue))
        {
            entity.CardAccessTimeGroups.Add(new CardAccessTimeGroups
            {
                TimeGroupId = entity.Id,
                CardAccessId = idAccess.Value,
                ApplicationId = entity.ApplicationId
            });
        }
    }

    // ============================================================
    // 6. SAVE (NO Update(entity) inside repo)
    // ============================================================
    await _repository.UpdateAsync(entity);
}





        public async Task DeleteAsync(Guid id)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("TimeGroup not found");
            foreach (var block in entity.TimeBlocks)
            {
                block.Status = 0;
                block.TimeGroupId = entity.Id;
                block.CreatedBy = username;
                block.UpdatedBy = username;
                block.CreatedAt = DateTime.UtcNow;
                block.UpdatedAt = DateTime.UtcNow;
            }


            entity.Status = 0;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;
            await _repository.DeleteAsync(id);
        }
        
        //  public async Task<object> FilterAsync(DataTablesRequest request)
        // {
        //     var query = _repository.GetAllQueryable();

        //     var searchableColumns = new[] { "Name" };
        //     var validSortColumns = new[] { "UpdatedAt", "Status", "Name" };

        //     var filterService = new GenericDataTableService<TimeGroup, TimeGroupDto>(
        //         query,
        //         _mapper,
        //         searchableColumns,
        //         validSortColumns);

        //     return await filterService.FilterAsync(request);
        // }
         public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.MinimalGetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Status", "Name" };

            var filterService = new MinimalGenericDataTableService<TimeGroupMinimalDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
    }

}