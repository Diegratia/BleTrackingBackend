using Entities.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;

namespace Repositories.Repository
{
    public class TrackingTransactionRepository : BaseRepository
    {
        private readonly IMapper _mapper;
        private readonly BleTrackingDbContext _context;

        public TrackingTransactionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
            : base(context, httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
        }

        private string GetCurrentTableName()
        {
            var wibNow = DateTime.UtcNow.AddHours(7);
            return $"tracking_transaction_{wibNow:yyyyMMdd}";
        }


        public async Task<TrackingTransaction?> GetByIdAsync(Guid id)
        {
            var tableName = GetCurrentTableName();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE Id = @p0", id)
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<TrackingTransaction?> GetByIdWithIncludesAsync(Guid id)
        {
            return await GetByIdAsync(id); // Sudah include
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllWithIncludesAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public IQueryable<TrackingTransaction> GetAllQueryable()
        {
            var tableName = GetCurrentTableName();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .IgnoreQueryFilters()
                .Include(t => t.Member)
                .Include(t => t.Visitor)
                .Include(t => t.Reader)
                .Include(t => t.Card)
                .Include(t => t.FloorplanMaskedArea);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public IQueryable<TrackingTransactionRM> GetProjectionQueryable(DateTime? from = null, DateTime? to = null)
        {
            var tableName = GetCurrentTableName();
            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking();

            if (from.HasValue && to.HasValue)
                query = query.Where(a => a.TransTime >= from && a.TransTime <= to);

            return query.ProjectTo<TrackingTransactionRM>(_mapper.ConfigurationProvider);
        }

        public IQueryable<TrackingTransactionRM> GetProjectionQueryableManual()
        {
            var tableName = GetCurrentTableName();
            return _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Select(t => new TrackingTransactionRM
                {
                    Id = t.Id,
                    TransTime = t.TransTime,
                    ReaderId = t.ReaderId,
                    ReaderName = t.Reader != null ? t.Reader.Name : null,
                    VisitorId = t.VisitorId,
                    VisitorName = t.Visitor != null ? t.Visitor.Name : null,
                    MemberId = t.MemberId,
                    MemberName = t.Member != null ? t.Member.Name : null,
                    FloorplanMaskedAreaId = t.FloorplanMaskedAreaId,
                    FloorplanMaskedAreaName = t.FloorplanMaskedArea != null ? t.FloorplanMaskedArea.Name : null,
                    AreaShape = t.FloorplanMaskedArea != null ? t.FloorplanMaskedArea.AreaShape : null,
                    AlarmStatus = t.AlarmStatus.HasValue ? t.AlarmStatus.ToString() : null,
                    Battery = t.Battery
                });
        }

        public IQueryable<TrackingTransactionWithAlarm> GetAllWithAlarmQueryable()
        {
            var tableName = GetCurrentTableName();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var trackingQuery = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .IgnoreQueryFilters()
                .Include(t => t.Reader)
                .Include(t => t.Card)
                .Include(t => t.Visitor)
                .Include(t => t.Member)
                .Include(t => t.FloorplanMaskedArea);

            var alarmQuery = _context.AlarmRecordTrackings
                .IgnoreQueryFilters()
                .Include(a => a.Reader)
                .Include(a => a.Visitor)
                .Include(a => a.Member)
                .Include(a => a.FloorplanMaskedArea);

            var joined = from tt in ApplyApplicationIdFilter(trackingQuery, applicationId, isSystemAdmin)
                         join ar in ApplyApplicationIdFilter(alarmQuery, applicationId, isSystemAdmin)
                             on tt.VisitorId equals ar.VisitorId into alarmGroup
                         from ar in alarmGroup.DefaultIfEmpty()
                         select new TrackingTransactionWithAlarm
                         {
                             Tracking = tt,
                             AlarmRecord = ar
                         };

            return joined;
        }

        public IQueryable<TrackingTransactionRM> GetTodayQueryable(DateTime? from = null, DateTime? to = null)
        {
            var tableName = GetCurrentTableName();
            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking();

            if (from.HasValue) query = query.Where(t => t.TransTime >= from);
            if (to.HasValue) query = query.Where(t => t.TransTime <= to);

            return query.ProjectTo<TrackingTransactionRM>(_mapper.ConfigurationProvider);
        }

        private async Task ValidateRelatedEntitiesAsync(TrackingTransaction transaction, Guid? applicationId, bool isSystemAdmin)
        {
            if (isSystemAdmin) return;

            if (!applicationId.HasValue)
                throw new UnauthorizedAccessException("Missing ApplicationId for non-admin.");

            var reader = await _context.MstBleReaders
                .FirstOrDefaultAsync(r => r.Id == transaction.ReaderId && r.ApplicationId == applicationId);
            if (reader == null)
                throw new UnauthorizedAccessException("Invalid ReaderId for this application.");

            var area = await _context.FloorplanMaskedAreas
                .FirstOrDefaultAsync(f => f.Id == transaction.FloorplanMaskedAreaId && f.ApplicationId == applicationId);
            if (area == null)
                throw new UnauthorizedAccessException("Invalid FloorplanMaskedAreaId for this application.");
        }

    }
}