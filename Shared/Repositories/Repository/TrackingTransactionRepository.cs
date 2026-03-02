using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Extensions;
using Shared.Contracts;
using Shared.Contracts.Read;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public class TrackingTransactionRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;

        public TrackingTransactionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
            _context = context;
        }

        private string GetCurrentTableName()
        {
            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return $"tracking_transaction_{wibZone:yyyyMMdd}";
        }

        private IQueryable<TrackingTransaction> BaseEntityQuery()
        {
            var tableName = GetCurrentTableName();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .Include(t => t.Reader)
                .Include(t => t.Member)
                .Include(t => t.Visitor)
                .Include(t => t.Card)
                .Include(t => t.FloorplanMaskedArea);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        private IQueryable<TrackingTransactionRead> ProjectToRead(IQueryable<TrackingTransaction> query)
        {
            return query.Select(t => new TrackingTransactionRead
            {
                Id = t.Id,
                ApplicationId = t.ApplicationId,
                TransTime = t.TransTime,
                ReaderId = t.ReaderId,
                ReaderName = t.Reader != null ? t.Reader.Name : null,
                CardId = t.CardId,
                VisitorId = t.VisitorId,
                VisitorName = t.Visitor != null ? t.Visitor.Name : null,
                MemberId = t.MemberId,
                MemberName = t.Member != null ? t.Member.Name : null,
                FloorplanMaskedAreaId = t.FloorplanMaskedAreaId,
                FloorplanMaskedAreaName = t.FloorplanMaskedArea != null ? t.FloorplanMaskedArea.Name : null,
                AreaShape = t.FloorplanMaskedArea != null ? t.FloorplanMaskedArea.AreaShape : null,
                CoordinateX = t.CoordinateX,
                CoordinateY = t.CoordinateY,
                CoordinatePxX = t.CoordinatePxX,
                CoordinatePxY = t.CoordinatePxY,
                AlarmStatus = t.AlarmStatus.HasValue ? t.AlarmStatus.ToString() : null,
                Battery = t.Battery
            });
        }

        public async Task<TrackingTransactionRead?> GetByIdAsync(Guid id)
        {
            var query = BaseEntityQuery();
            return await ProjectToRead(query).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<TrackingTransaction?> GetByIdEntityAsync(Guid id)
        {
            return await BaseEntityQuery().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<TrackingTransactionRead>> GetAllAsync()
        {
            return await ProjectToRead(BaseEntityQuery()).ToListAsync();
        }

        public async Task<(List<TrackingTransactionRead> Data, int Total, int Filtered)> FilterAsync(TrackingTransactionFilter filter)
        {
            var query = BaseEntityQuery();

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(x =>
                    (x.Reader != null && x.Reader.Name.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.Visitor != null && x.Visitor.Name.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.Member != null && x.Member.Name.ToLower().Contains(filter.Search.ToLower())) ||
                    (x.FloorplanMaskedArea != null && x.FloorplanMaskedArea.Name.ToLower().Contains(filter.Search.ToLower())));

            if (filter.ReaderId.HasValue)
                query = query.Where(x => x.ReaderId == filter.ReaderId.Value);

            if (filter.CardId.HasValue)
                query = query.Where(x => x.CardId == filter.CardId.Value);

            if (filter.VisitorId.HasValue)
                query = query.Where(x => x.VisitorId == filter.VisitorId.Value);

            if (filter.MemberId.HasValue)
                query = query.Where(x => x.MemberId == filter.MemberId.Value);

            if (filter.FloorplanMaskedAreaId.HasValue)
                query = query.Where(x => x.FloorplanMaskedAreaId == filter.FloorplanMaskedAreaId.Value);

            if (!string.IsNullOrWhiteSpace(filter.AlarmStatus))
                query = query.Where(x => x.AlarmStatus.ToString() == filter.AlarmStatus);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.TransTime >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.TransTime <= filter.DateTo.Value);

            var total = await query.CountAsync();
            var filtered = total;

            query = query.ApplySorting(filter.SortColumn, filter.SortDir);
            query = query.ApplyPaging(filter.Page, filter.PageSize);

            var data = await ProjectToRead(query).ToListAsync();

            return (data, total, filtered);
        }

        public async Task<List<TrackingTransaction>> GetAllWithIncludesAsync()
        {
            return await BaseEntityQuery().ToListAsync();
        }

        public async Task<List<TrackingTransaction>> GetAllExportAsync()
        {
            return await BaseEntityQuery()
                .Include(t => t.Card)
                .ToListAsync();
        }

        public IQueryable<TrackingTransactionWithAlarm> GetAllWithAlarmQueryable()
        {
            var tableName = GetCurrentTableName();
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var trackingQuery = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .Include(t => t.Reader)
                .Include(t => t.Card)
                .Include(t => t.Visitor)
                .Include(t => t.Member)
                .Include(t => t.FloorplanMaskedArea);

            var alarmQuery = _context.AlarmRecordTrackings
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
    }
}
