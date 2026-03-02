
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Helpers.Consumer.DtoHelpers.MinimalDto;
using Shared.Contracts.Read;

namespace Repositories.Repository
{
    public class TimeGroupRepository : BaseRepository
    {
        public TimeGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        /// <summary>
        /// Base query for TimeGroup with multi-tenancy filtering and status check
        /// </summary>
        public IQueryable<TimeGroup> BaseEntityQuery()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();
            var query = _context.TimeGroups
                .Include(t => t.TimeBlocks)
                .Include(t => t.CardAccessTimeGroups)
                .Where(t => t.Status != 0);
            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        /// <summary>
        /// Manual projection to TimeGroupRead DTO
        /// </summary>
        private IQueryable<TimeGroupRead> ProjectToRead(IQueryable<TimeGroup> query)
        {
            return query.Select(tg => new TimeGroupRead
            {
                Id = tg.Id,
                Name = tg.Name,
                Description = tg.Description,
                ScheduleType = tg.ScheduleType,
                ApplicationId = tg.ApplicationId,
                Status = tg.Status,
                CreatedBy = tg.CreatedBy,
                CreatedAt = tg.CreatedAt,
                UpdatedBy = tg.UpdatedBy,
                UpdatedAt = tg.UpdatedAt,
                CardAccessIds = tg.CardAccessTimeGroups
                    .Select(x => (Guid?)x.CardAccessId)
                    .ToList(),
                TimeBlocks = tg.TimeBlocks
                    .Where(tb => tb.Status != 0)
                    .Select(tb => new TimeBlockRead
                    {
                        Id = tb.Id,
                        DayOfWeek = tb.DayOfWeek,
                        StartTime = tb.StartTime,
                        EndTime = tb.EndTime
                    })
                    .ToList()
            });
        }

        /// <summary>
        /// Validates that CardAccessIds belong to the same Application
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidCardAccessOwnershipAsync(
            List<Guid?> cardAccessIds, Guid applicationId)
        {
            var validIds = cardAccessIds.Where(x => x.HasValue).Select(x => x.Value).ToList();
            if (!validIds.Any())
                return Array.Empty<Guid>();

            return await CheckInvalidOwnershipIdsAsync<CardAccess>(
                validIds, applicationId);
        }

        /// <summary>
        /// Validates that TimeGroupIds belong to the same Application
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> CheckInvalidTimeGroupOwnershipAsync(
            List<Guid?> timeGroupIds, Guid applicationId)
        {
            var validIds = timeGroupIds.Where(x => x.HasValue).Select(x => x.Value).ToList();
            if (!validIds.Any())
                return Array.Empty<Guid>();

            return await CheckInvalidOwnershipIdsAsync<TimeGroup>(
                validIds, applicationId);
        }

        /// <summary>
        /// Gets TimeGroup as entity (for update/delete operations)
        /// </summary>
        public async Task<TimeGroup?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Gets TimeGroup as TimeGroupRead DTO
        /// </summary>
        public async Task<TimeGroupRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery().Where(t => t.Id == id);
            return await ProjectToRead(query).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TimeGroupRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<TimeGroup> AddAsync(TimeGroup entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                entity.ApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            _context.TimeGroups.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Adds TimeGroup and returns the TimeGroupRead DTO directly
        /// </summary>
        public async Task<TimeGroupRead> AddAndReturnAsync(TimeGroup entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var targetApplicationId = applicationId;
            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId not found in context");

                entity.ApplicationId = applicationId.Value;
                targetApplicationId = applicationId.Value;
            }
            else if (entity.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("System admin must provide a valid ApplicationId");
            }
            else
            {
                targetApplicationId = entity.ApplicationId;
            }

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            // Set ApplicationId on child entities
            foreach (var block in entity.TimeBlocks)
            {
                if (block.ApplicationId == Guid.Empty)
                    block.ApplicationId = targetApplicationId.Value;
            }

            foreach (var catg in entity.CardAccessTimeGroups ?? Enumerable.Empty<CardAccessTimeGroups>())
            {
                if (catg.ApplicationId == Guid.Empty)
                    catg.ApplicationId = targetApplicationId.Value;
            }

            _context.TimeGroups.Add(entity);
            await _context.SaveChangesAsync();

            // Reload the entity with includes to ensure complete data
            await _context.Entry(entity).Collection(t => t.TimeBlocks).LoadAsync();
            await _context.Entry(entity).Collection(t => t.CardAccessTimeGroups).LoadAsync();

            // Project manually to avoid query issues
            return new TimeGroupRead
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ScheduleType = entity.ScheduleType,
                ApplicationId = entity.ApplicationId,
                Status = entity.Status,
                CreatedBy = entity.CreatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                CardAccessIds = entity.CardAccessTimeGroups?
                    .Select(x => (Guid?)x.CardAccessId)
                    .ToList() ?? new List<Guid?>(),
                TimeBlocks = entity.TimeBlocks?
                    .Where(tb => tb.Status != 0)
                    .Select(tb => new TimeBlockRead
                    {
                        Id = tb.Id,
                        DayOfWeek = tb.DayOfWeek,
                        StartTime = tb.StartTime,
                        EndTime = tb.EndTime
                    })
                    .ToList() ?? new List<TimeBlockRead>()
            };
        }

        public async Task UpdateAsync(TimeGroup entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var entity = await BaseEntityQuery()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
                throw new KeyNotFoundException("TimeGroup not found");

            // Soft delete - set Status to 0
            entity.Status = 0;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.TimeGroups.Update(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<TimeGroupMinimalDto> MinimalGetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Where(d => d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query.Select(tg => new TimeGroupMinimalDto
            {
                Id = tg.Id,
                Name = tg.Name,
                Description = tg.Description,
                ScheduleType = tg.ScheduleType,
                ApplicationId = tg.ApplicationId,
                UpdatedAt = tg.UpdatedAt,

                CardAccessIds = tg.CardAccessTimeGroups
                    .Select(x => (Guid?)x.CardAccessId)
                    .ToList(),

                TimeBlocks = tg.TimeBlocks
                    .Where(tb => tb.Status != 0)
                    .Select(tb => new TimeBlockMinimalDto
                    {
                        Id = tb.Id,
                        DayOfWeek = tb.DayOfWeek.HasValue ? tb.DayOfWeek.Value.ToString() : null,
                        ApplicationId = tb.ApplicationId,
                        TimeGroupId = tb.TimeGroupId,
                        StartTime = tb.StartTime,
                        EndTime = tb.EndTime
                    })
                    .ToList()
            });
        }

        public async Task<IEnumerable<TimeGroup>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Where(d => d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }
    }
}
