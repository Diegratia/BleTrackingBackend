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
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class TimeGroupService : ITimeGroupService
    {
        private readonly TimeGroupRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimeGroupService(
            TimeGroupRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
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
            return _mapper.Map<TimeGroupDto>(result);
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



        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;
            entity.Status = 0;
            await _repository.DeleteAsync(id);
            return true;
        }
        
         public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "Name", "Tag", "Status" };

            var filterService = new GenericDataTableService<TimeGroup, TimeGroupDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
    }

}