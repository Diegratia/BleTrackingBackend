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
        public TrackingTransactionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
            : base(context, httpContextAccessor)
        {
            _mapper = mapper;
        }

        public async Task<TrackingTransaction?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingTransactions
                .Include(t => t.Reader)
                .Include(t => t.FloorplanMaskedArea)
                .Where(t => t.Id == id);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        public async Task<TrackingTransaction?> GetByIdWithIncludesAsync(Guid id)
        {
            return await GetByIdAsync(id); // sudah include
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllWithIncludesAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<IEnumerable<TrackingTransaction>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task AddAsync(TrackingTransaction transaction)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin)
            {
                if (!applicationId.HasValue)
                    throw new UnauthorizedAccessException("ApplicationId required for non-admin users.");

                transaction.ApplicationId = applicationId.Value;
            }
            else if (transaction.ApplicationId == Guid.Empty)
            {
                throw new ArgumentException("SystemAdmin Must provide ApplicationId.");
            }

            await ValidateApplicationIdAsync(transaction.ApplicationId);
            ValidateApplicationIdForEntity(transaction, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(transaction, applicationId, isSystemAdmin);

            _context.TrackingTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrackingTransaction transaction)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(transaction.ApplicationId);
            ValidateApplicationIdForEntity(transaction, applicationId, isSystemAdmin);
            await ValidateRelatedEntitiesAsync(transaction, applicationId, isSystemAdmin);

            _context.TrackingTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TrackingTransaction transaction)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && transaction.ApplicationId != applicationId)
                throw new UnauthorizedAccessException("You don’t have permission to delete this transaction.");

            _context.TrackingTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public IQueryable<TrackingTransaction> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingTransactions
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
            var query = _context.TrackingTransactions
                .AsNoTracking();

            // ✅ Filter waktu di entity langsung, bukan DTO
            if (from.HasValue && to.HasValue)
                query = query.Where(a => a.TransTime >= from && a.TransTime <= to);

            // ✅ Baru di-project ke DTO
            return query.ProjectTo<TrackingTransactionRM>(_mapper.ConfigurationProvider);
        }

        public IQueryable<TrackingTransactionRM> GetProjectionQueryableManual()
{
    return _context.TrackingTransactions
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
            CoordinateX = t.CoordinateX,
            CoordinateY = t.CoordinateY,
            AlarmStatus = t.AlarmStatus.HasValue ? t.AlarmStatus.ToString() : null,
            Battery = t.Battery
        });
}


        
        public IQueryable<TrackingTransactionWithAlarm> GetAllWithAlarmQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var trackingQuery = _context.TrackingTransactions
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

            // join berdasarkan VisitorId — bisa diubah ke CardId jika lebih akurat
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
