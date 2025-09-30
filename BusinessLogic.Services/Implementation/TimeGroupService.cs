using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Entities.Models;
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
            return _mapper.Map<IEnumerable<TimeGroupDto>>(entities);
        }

        public async Task<TimeGroupDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<TimeGroupDto>(entity);
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

            // Inject CardAccesses (optional)
            if (dto.CardAccessIds.Any())
            {
                var validAccesses = await _cardAccessRepository.GetAllQueryable()
                    .Where(ca => dto.CardAccessIds.Contains(ca.Id))
                    .Select(ca => ca.Id)
                    .ToListAsync();

                if (validAccesses.Count != dto.CardAccessIds.Count)
                    throw new ArgumentException("Some CardAccessIds are invalid.");

                entity.CardAccessTimeGroups = validAccesses
                    .Select(id => new CardAccessTimeGroups
                    {
                        TimeGroupId = entity.Id,
                        CardAccessId = id,
                        ApplicationId = entity.ApplicationId
                    })
                    .ToList();
            }

            entity.Id = Guid.NewGuid();
            entity.CreatedBy = username;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

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
            entity.Status = 1;

            var result = await _repository.AddAsync(entity);

            var dtoResult = _mapper.Map<TimeGroupDto>(result);
            dtoResult.CardAccessIds = entity.CardAccessTimeGroups?
                .Select(x => (Guid?)x.CardAccessId)
                .ToList();

            return dtoResult;
        }

         public async Task UpdateAsync(Guid id, TimeGroupUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException();

            // map property scalar saja (jangan map koleksi TimeBlocks)
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.UpdatedBy = username;
            entity.UpdatedAt = DateTime.UtcNow;

            // update existing blocks
            foreach (var blockDto in dto.TimeBlocks.Where(x => x.Id != Guid.Empty))
            {
                var existing = entity.TimeBlocks.FirstOrDefault(b => b.Id == blockDto.Id);
                if (existing != null)
                {
                    existing.DayOfWeek = !string.IsNullOrEmpty(blockDto.DayOfWeek)
                        ? Enum.Parse<DayOfWeek>(blockDto.DayOfWeek, true)
                        : null;
                    existing.StartTime = blockDto.StartTime;
                    existing.EndTime = blockDto.EndTime;
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = username;
                }
            }

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
        
         public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Status", "Name" };

            var filterService = new GenericDataTableService<TimeGroup, TimeGroupDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
    }

}