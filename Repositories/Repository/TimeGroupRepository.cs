
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Repositories.DbContexts;
using Helpers.Consumer.DtoHelpers.MinimalDto;

namespace Repositories.Repository
{
    public class TimeGroupRepository : BaseRepository
    {
        public TimeGroupRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

            public async Task<TimeGroup?> GetByIdAsync(Guid id)
            {
                return await _context.TimeGroups
                    .Include(t => t.TimeBlocks)
                    .Include(t => t.CardAccessTimeGroups)
                    .FirstOrDefaultAsync(t => t.Id == id);

            }

            public async Task<IEnumerable<TimeGroup>> GetAllAsync()
            {
                return await GetAllQueryable()
                .ToListAsync();
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

        public async Task UpdateAsync(TimeGroup entity)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(entity.ApplicationId);
            ValidateApplicationIdForEntity(entity, applicationId, isSystemAdmin);

            _context.TimeGroups.Update(entity); // Optional
                foreach (var entry in _context.ChangeTracker.Entries())
            {
                Console.WriteLine($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Include(d => d.TimeBlocks)
                .Include(ca => ca.CardAccessTimeGroups)
                .Where(d => d.Id == id && d.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var entity = await query.FirstOrDefaultAsync();
            if (entity == null)
                throw new KeyNotFoundException("TimeGroup not found");
                
            await _context.SaveChangesAsync();
        }

        public IQueryable<TimeGroup> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TimeGroups
                .Include(d => d.TimeBlocks)
                .Include(ca => ca.CardAccessTimeGroups)
                .Where(d => d.Status != 0);

                

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
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
                ScheduleType = tg.ScheduleType.ToString(),
                ApplicationId = tg.ApplicationId,
                UpdatedAt = tg.UpdatedAt,

                // langsung ambil list CardAccessId
                CardAccessIds = tg.CardAccessTimeGroups
                    .Select(x => (Guid?)x.CardAccessId)
                    .ToList(),

                // kalau mau minimal juga bisa expose TimeBlocks
                TimeBlocks = tg.TimeBlocks
                    .Where(tb => tb.Status != 0) // filter only active
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
        
public void RemoveTimeBlock(TimeBlock block)
{
    if (block == null) return;

    var entry = _context.Entry(block);

    if (entry.State == EntityState.Detached)
        _context.TimeBlocks.Attach(block);

    _context.TimeBlocks.Remove(block);
}

public void AddTimeBlock(TimeBlock block)
{
    _context.TimeBlocks.Add(block);
}



         public IQueryable<CardAccessMinimalDto> MinimalGetAllQueryableDto()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardAccesses
            .Include(ca => ca.CardAccessTimeGroups)
                .ThenInclude(ca => ca.TimeGroup)
            .Include(ca => ca.CardAccessMaskedAreas)
                .ThenInclude(cam => cam.MaskedArea)
            .Where(ca => ca.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return query.Select(ca => new CardAccessMinimalDto
            {
                Id = ca.Id,
                Name = ca.Name,
                AccessNumber = ca.AccessNumber ?? 0,
                AccessScope = ca.AccessScope.ToString(),
                Remarks = ca.Remarks,
                UpdatedAt = ca.UpdatedAt,
                ApplicationId = ca.ApplicationId,

                MaskedAreaIds = ca.CardAccessMaskedAreas
                        .Select(x => (Guid?)x.MaskedAreaId)
                        .ToList(),

                TimeGroupIds = ca.CardAccessTimeGroups
                        .Select(x => (Guid?)x.TimeGroupId)
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
