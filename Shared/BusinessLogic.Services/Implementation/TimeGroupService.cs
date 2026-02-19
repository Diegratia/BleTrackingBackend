using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using DataView;
using Entities.Models;
using Shared.Contracts;
using Shared.Contracts.Read;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository;
using Repositories.DbContexts;
using Helpers.Consumer.DtoHelpers.MinimalDto;

namespace BusinessLogic.Services.Implementation
{
    public class TimeGroupService : BaseService, ITimeGroupService
    {
        private readonly TimeGroupRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CardAccessRepository _cardAccessRepository;
        private readonly BleTrackingDbContext _context;
        private readonly IAuditEmitter _audit;

        public TimeGroupService(
            TimeGroupRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            CardAccessRepository cardAccessRepository,
            BleTrackingDbContext context,
            IAuditEmitter audit
        ) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cardAccessRepository = cardAccessRepository;
            _context = context;
            _audit = audit;
        }

        public async Task<IEnumerable<TimeGroupRead>> GetAllsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<TimeGroupRead?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<TimeGroupRead> CreateAsync(TimeGroupCreateDto dto)
        {
            var entity = _mapper.Map<TimeGroup>(dto);
            SetCreateAudit(entity);

            // Validate CardAccessIds ownership
            if (dto.CardAccessIds.Any())
            {
                var invalidIds = await _repository.CheckInvalidCardAccessOwnershipAsync(
                    dto.CardAccessIds, AppId);
                if (invalidIds.Any())
                    throw new UnauthorizedException(
                        $"CardAccessIds do not belong to this Application: {string.Join(", ", invalidIds)}");

                entity.CardAccessTimeGroups = dto.CardAccessIds
                    .Where(x => x.HasValue)
                    .Select(id => new CardAccessTimeGroups
                    {
                        TimeGroupId = entity.Id,
                        CardAccessId = id.Value,
                        ApplicationId = AppId
                    })
                    .ToList();
            }

            // Set audit and properties for nested TimeBlocks
            foreach (var block in entity.TimeBlocks)
            {
                block.Status = 1;
                block.ApplicationId = AppId;
                block.TimeGroupId = entity.Id;
                block.CreatedBy = UsernameFormToken;
                block.UpdatedBy = UsernameFormToken;
                block.CreatedAt = DateTime.UtcNow;
                block.UpdatedAt = DateTime.UtcNow;
            }
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CreatedBy = UsernameFormToken;
            entity.UpdatedBy = UsernameFormToken;
            entity.Status = 1;

            var result = await _repository.AddAndReturnAsync(entity);
            _audit.Created("TimeGroup", entity.Id, $"TimeGroup {entity.Name} created");

            return result;
        }

        public async Task UpdateAsync(Guid id, TimeGroupUpdateDto dto)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("TimeGroup not found");

            // Update scalar fields
            entity.Name = dto.Name ?? entity.Name;
            entity.Description = dto.Description ?? entity.Description;
            SetUpdateAudit(entity);

            // Validate CardAccessIds ownership
            if (dto.CardAccessIds != null)
            {
                var invalidIds = await _repository.CheckInvalidCardAccessOwnershipAsync(
                    dto.CardAccessIds, AppId);
                if (invalidIds.Any())
                    throw new UnauthorizedException(
                        $"CardAccessIds do not belong to this Application: {string.Join(", ", invalidIds)}");
            }

            // Sync TimeBlocks - soft delete all existing, then add new
            var currentBlocks = entity.TimeBlocks.ToList();
            foreach (var block in currentBlocks)
            {
                block.Status = 0; // Soft delete
                block.UpdatedAt = DateTime.UtcNow;
                block.UpdatedBy = UsernameFormToken;
            }

            foreach (var tb in dto.TimeBlocks.Where(t => t != null))
            {
                var newBlock = new TimeBlock
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = !string.IsNullOrEmpty(tb.DayOfWeek)
                        ? Enum.Parse<DayOfWeek>(tb.DayOfWeek, true)
                        : null,
                    StartTime = tb.StartTime,
                    EndTime = tb.EndTime,
                    Status = 1,
                    ApplicationId = AppId,
                    TimeGroupId = entity.Id,
                    CreatedBy = UsernameFormToken,
                    UpdatedBy = UsernameFormToken,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.TimeBlocks.Add(newBlock);
            }

            // Sync CardAccessTimeGroups
            entity.CardAccessTimeGroups.Clear();
            if (dto.CardAccessIds != null)
            {
                foreach (var idAccess in dto.CardAccessIds.Where(x => x.HasValue))
                {
                    entity.CardAccessTimeGroups.Add(new CardAccessTimeGroups
                    {
                        TimeGroupId = entity.Id,
                        CardAccessId = idAccess.Value,
                        ApplicationId = AppId
                    });
                }
            }

            await _repository.UpdateAsync(entity);
            _audit.Updated("TimeGroup", entity.Id, $"TimeGroup {entity.Name} updated");
        }





        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdEntityAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("TimeGroup not found");

            SetDeleteAudit(entity);

            // Soft delete related TimeBlocks
            foreach (var block in entity.TimeBlocks)
            {
                block.Status = 0;
                block.UpdatedAt = DateTime.UtcNow;
                block.UpdatedBy = UsernameFormToken;
            }

            await _repository.DeleteAsync(id);
            _audit.Deleted("TimeGroup", id, $"TimeGroup {entity.Name} deleted");
        }

        public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.MinimalGetAllQueryable();

            var searchableColumns = new[] { "Name" };
            var validSortColumns = new[] { "UpdatedAt", "Status", "Name", "ScheduleType" };

            var enumColumns = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "ScheduleType", typeof(ScheduleType) }
            };

            var filterService = new MinimalGenericDataTableService<TimeGroupMinimalDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns,
                enumColumns);

            return await filterService.FilterAsync(request);
        }
    }
}