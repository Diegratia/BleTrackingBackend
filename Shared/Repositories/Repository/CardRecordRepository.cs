using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Helpers.Consumer;
using Data.ViewModels;
using Repositories.Repository.RepoModel;
using Repositories.Models;

namespace Repositories.Repository
{
    public class CardRecordRepository : BaseProjectionRepository<CardRecord, CardRecordListRM>
    {
        public CardRecordRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
           
        }

        public async Task<CardRecord?> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardRecords
                .Include(a => a.Visitor)
                .Include(a => a.Card)
                .Include(a => a.Member)
                .Include(a => a.Application)
                .Where(b => b.Id == id);

            return await ApplyApplicationIdFilter(query, applicationId, isSystemAdmin).FirstOrDefaultAsync();
        }

        //  public IQueryable<CardRecordDtoz> GetAllQueryableMinimal()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.CardRecords
        //         .Include(a => a.Visitor)
        //         .Include(a => a.Card)
        //         .Include(a => a.Member)
        //         .Where(b => b.Status != 0)
        //         .AsQueryable();

        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

        //     return query.Select(v => new CardRecordDtoz
        //     {
        //         Id = v.Id,
        //         VisitorName = v.Name,
        //         Visitor = v.Visitor == null ? null : new VisitorDtoz
        //         {
        //             Id = v.Visitor.Id,
        //             Name = v.Visitor.Name
        //         },
        //     });
        // }


         // ================================
        // 1️⃣ Berapa kali kartu dipakai
        // ================================
        public async Task<List<CardUsageSummaryRM>> GetCardUsageSummaryAsync()
        {
            // var from = request.From;
            // var to = request.To;

            var query = _context.CardRecords
                .AsNoTracking()
                .Include(x => x.Card)
                .Where(x => x.CardId != null);

            // if (from.HasValue)
            //     query = query.Where(x => x.UpdatedAt >= from);

            // if (to.HasValue)
            //     query = query.Where(x => x.UpdatedAt <= to);

            return await query
                .GroupBy(x => new
                {
                    x.CardId,
                    x.Card.CardNumber
                })
                .Select(g => new CardUsageSummaryRM
                {
                    CardId = g.Key.CardId,
                    CardNumber = g.Key.CardNumber,
                    LastUsedBy = g
                        .OrderByDescending(x => x.UpdatedAt)
                        .Select(x => x.Visitor != null
                            ? x.Card.Visitor.Name
                            : x.Card.Member != null
                                ? x.Member.Name
                                : x.Name)
                        .FirstOrDefault(),
                    Status = g
                        .OrderByDescending(x => x.UpdatedAt)
                        .Select(x => x.CheckoutAt == null ? "Active" : "Non Active")
                        .FirstOrDefault(),
                    TotalUsage = g.Count() 
                })
                .OrderByDescending(x => x.TotalUsage)
                .ToListAsync();
        }

        // ================================
        // 2️⃣ Historis kartu dipakai siapa
        // ================================
            public async Task<List<CardUsageHistoryRM>> GetCardUsageHistoryAsync(
                CardRecordRequestRM request
            )
            {
                   // 🕒 Time range
                var range = GetTimeRange(request.TimeRange);
                var from = range?.from ?? request.From;
                var to = range?.to ?? request.To;
                var query = _context.CardRecords
                    .AsNoTracking()
                    .Include(x => x.Card)
                    .Include(x => x.Visitor)
                    .Include(x => x.Member)
                    .Where(x => x.CardId == request.CardId);

                if (from.HasValue)
                    query = query.Where(x => x.Timestamp >= from);

                if (to.HasValue)
                    query = query.Where(x => x.Timestamp <= to);

                return await query
                    .OrderByDescending(x => x.Timestamp)
                    .Select(x => new CardUsageHistoryRM
                    {
                        CardId = x.CardId,
                        IdentityId = x.Visitor != null
                            ? x.Visitor.IdentityId
                            : x.Member != null
                                ? x.Member.IdentityId
                                : null,
                        CardNumber = x.Card.CardNumber,
                        UsedBy = x.Visitor != null
                            ? x.Visitor.Name
                            : x.Member != null
                                ? x.Member.Name
                                : x.Name,
                        UsedByType = x.Visitor != null
                            ? "Visitor"
                            : x.Member != null
                                ? "Member"
                                : "Unknown",
                        FaceImage = x.Visitor != null
                            ? x.Visitor.FaceImage
                            : x.Member != null
                                ? x.Member.FaceImage
                                : null,
                        CheckinAt = x.CheckinAt,
                        CheckoutAt = x.CheckoutAt
                    })
                    .ToListAsync();
            }

        public async Task<IEnumerable<CardRecord>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

         public async Task<CardRecord> AddAsync(CardRecord cardRecord)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

                // non system ambil dari claim
                if (!isSystemAdmin)
                {
                    if (!applicationId.HasValue)
                        throw new UnauthorizedAccessException("ApplicationId not found in context");
                    cardRecord.ApplicationId = applicationId.Value;
                }
                // admin set applciation di body
                else if (cardRecord.ApplicationId == Guid.Empty)
                {
                    throw new ArgumentException("System admin must provide a valid ApplicationId");
                }
            await ValidateApplicationIdAsync(cardRecord.ApplicationId);
            ValidateApplicationIdForEntity(cardRecord, applicationId, isSystemAdmin);
            
            _context.CardRecords.Add(cardRecord);
            await _context.SaveChangesAsync();
            return cardRecord;
        }

            public async Task UpdateAsync(CardRecord cardRecord)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            await ValidateApplicationIdAsync(cardRecord.ApplicationId);
            ValidateApplicationIdForEntity(cardRecord, applicationId, isSystemAdmin);

            await _context.SaveChangesAsync();
        }

        
        //  public IQueryable<CardUsageHistory> GetAllQueryableMinimal()
        // {
        //     var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        //     var query = _context.CardRecords
        //         .Include(a => a.Visitor)
        //         .Include(a => a.Card)
        //         .Include(a => a.Member)
        //         .Where(b => b.Status != 0)
        //         .AsQueryable();

        //     query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

        //     return query.Select(v => new CardRecordDtoz
        //     {
        //         Id = v.Id,
        //         VisitorName = v.Name,
        //         Visitor = v.Visitor == null ? null : new VisitorDtoz
        //         {
        //             Id = v.Visitor.Id,
        //             Name = v.Visitor.Name
        //         },
        //     });
        // }


        public IQueryable<CardRecord> GetAllQueryable()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.CardRecords
                .Include(a => a.Visitor)
                .Include(a => a.Card)
                .Include(a => a.Member)
                .Include(a => a.Application);

            return ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);
        }

        public async Task<IEnumerable<CardRecord>> GetAllExportAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<Visitor?> GetVisitorByIdAsync(Guid id)
        {
            return await _context.Visitors
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<MstMember?> GetMemberByIdAsync(Guid id)
        {
            return await _context.MstMembers
                .FirstOrDefaultAsync(a => a.Id == id && a.Status != 0);
        }

        public async Task<Card?> GetCardByIdAsync(Guid id)
        {
            return await _context.Cards
                .FirstOrDefaultAsync(a => a.Id == id && a.StatusCard != 0);
        }

          private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
            {
                if (string.IsNullOrWhiteSpace(timeReport))
                    return null;

                // Gunakan UTC agar konsisten untuk server analytics
                var now = DateTime.UtcNow;

                // Pastikan format switch case aman (lowercase)
                return timeReport.Trim().ToLower() switch
                {
                    "daily" => (
                        now.Date,
                        now.Date.AddDays(1).AddTicks(-1)
                    ),

                    "weekly" => (
                        now.Date.AddDays(-(int)now.DayOfWeek + 1),                // Senin awal minggu
                        now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1) // Minggu akhir
                    ),

                    "monthly" => (
                        new DateTime(now.Year, now.Month, 1),
                        new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
                            .AddDays(1).AddTicks(-1)
                    ),

                    "yearly" => (
                        new DateTime(now.Year, 1, 1),
                        new DateTime(now.Year, 12, 31)
                            .AddDays(1).AddTicks(-1)
                    ),

                    _ => null
                };
            }

             protected override IQueryable<CardRecordListRM> Project(
        IQueryable<CardRecord> query)
    {
        return query
            .Include(x => x.Card)
            .Include(x => x.Visitor)
            .Include(x => x.Member)
            .Where(x => x.CheckinAt != null)
            .Select(x => new CardRecordListRM
            {
                Id = x.Id,
                CardId = x.CardId.Value,
                CardNumber = x.Card.CardNumber,
                IdentityId = x.Visitor != null
                    ? x.Visitor.IdentityId
                    : x.Member != null
                        ? x.Member.IdentityId
                        : null,
                PersonName = x.Visitor != null
                    ? x.Visitor.Name
                    : x.Member != null
                        ? x.Member.Name
                        : x.Name,
                PersonType = x.Visitor != null
                    ? "Visitor"
                    : x.Member != null
                        ? "Member"
                        : "Unknown",
                FaceImage = x.Visitor != null
                    ? x.Visitor.FaceImage
                    : x.Member != null
                        ? x.Member.FaceImage
                        : null,
                CheckinAt = x.CheckinAt.Value,
                CheckoutAt = x.CheckoutAt,
                UpdatedAt = x.UpdatedAt
            });
    }

    // =====================================================
    // OVERRIDE PAGED RESULT → tambah Duration & Status
    // =====================================================
    public override async Task<(List<CardRecordListRM> Data, long TotalRecords, long FilteredRecords)>
        GetPagedResultAsync(
            Dictionary<string, object> filters,
            Dictionary<string, DateRangeFilter>? dateFilters = null,
            string? timeReport = null,
            string sortColumn = "TimeStamp",
            string sortDir = "desc",
            int start = 0,
            int length = 10)
    {
        var (data, total, filtered) =
            await base.GetPagedResultAsync(
                filters,
                dateFilters,
                timeReport,
                sortColumn,
                sortDir,
                start,
                length);

        var now = DateTime.UtcNow;

        foreach (var row in data)
        {
            var end = row.CheckoutAt ?? now;
            var duration = end - row.CheckinAt;

            row.Duration = FormatDuration(duration);

            if (row.CheckoutAt == null)
                row.Status = "Active";
            else
                row.Status = "Checked Out";
                    }

        return (data, total, filtered);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
            return $"{duration.Seconds}s";

        if (duration.TotalHours < 1)
            return $"{duration.Minutes}m {duration.Seconds}s";

        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }
    }
}
