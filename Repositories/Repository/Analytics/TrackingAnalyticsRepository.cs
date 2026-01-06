using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using Repositories.Repository.RepoModel;
using Helpers.Consumer;

using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Repositories.Repository.Analytics
{
    public class TrackingAnalyticsRepository : BaseRepository
    {
        private readonly BleTrackingDbContext _context;
        private static readonly TimeZoneInfo WibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public TrackingAnalyticsRepository(BleTrackingDbContext context, IHttpContextAccessor accessor)
            : base(context, accessor)
        {
            _context = context;
        }

        // get wib table name
        private static string GetTableNameByDate(DateTime utcDate)
        {
            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var wibDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);
            // var wibDate = utcDate.AddHours(7);
            return $"tracking_transaction_{wibDate:yyyyMMdd}";
        }

        // apply time tange
        private (DateTime from, DateTime to)? GetTimeRange(string? timeReport)
        {
            if (string.IsNullOrWhiteSpace(timeReport))
                return null;

            var now = DateTime.UtcNow;
            return timeReport.Trim().ToLower() switch
            {
                "daily" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "weekly" => (
                    now.Date.AddDays(-(int)now.DayOfWeek + 1),
                    now.Date.AddDays(7 - (int)now.DayOfWeek).AddDays(1).AddTicks(-1)
                ),
                "monthly" => (
                    new DateTime(now.Year, now.Month, 1),
                    new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
                        .AddDays(1).AddTicks(-1)
                ),
                _ => null
            };
        }

        // area sum
        public async Task<List<TrackingAreaSummaryRM>> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;
            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Include(t => t.FloorplanMaskedArea)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            query = ApplyFilters(query, request);

            var data = await query
                .Select(t => new
                {
                    t.CardId,
                    t.FloorplanMaskedAreaId,
                    AreaName = t.FloorplanMaskedArea.Name
                })
                .Distinct()
                .GroupBy(x => new { x.FloorplanMaskedAreaId, x.AreaName })
                .Select(g => new TrackingAreaSummaryRM
                {
                    AreaId = g.Key.FloorplanMaskedAreaId,
                    AreaName = g.Key.AreaName,
                    // jumlah card unik per area
                    TotalRecords = g.Count()
                })
                .OrderByDescending(x => x.TotalRecords)
                .ToListAsync();

            return data;
        }

        // count area accessed sum
        public async Task<TrackingAccessPermissionSummaryRM> GetAccessPermissionSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;
            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Include(t => t.FloorplanMaskedArea)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            query = ApplyFilters(query, request);

            // Ambil area unik yang dikunjungi
            var data = await query
                .Select(t => new
                {
                    t.FloorplanMaskedAreaId,
                    RestrictedStatus = t.FloorplanMaskedArea.RestrictedStatus
                })
                .Distinct()
                .ToListAsync();

            var totalWithPermission = data.Count(x => x.RestrictedStatus == RestrictedStatus.NonRestrict);
            var totalWithoutPermission = data.Count(x => x.RestrictedStatus == RestrictedStatus.Restrict);

            return new TrackingAccessPermissionSummaryRM
            {
                AccessedAreaTotal = data.Count,
                WithPermission = totalWithPermission,
                WithoutPermission = totalWithoutPermission
            };
        }

        public async Task<TrackingAccessPermissionSummaryRM> GetAccessPermissionSummaryAsyncV2(TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var fromUtc = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var toUtc = range?.to ?? request.To ?? DateTime.UtcNow;

            var tableNames = GetTableNamesInRange(fromUtc, toUtc);
            if (!tableNames.Any())
                return new TrackingAccessPermissionSummaryRM();

            var unionParts = new List<string>();
            var parameters = new List<object>();
            int pIndex = 0;

            foreach (var table in tableNames)
            {
                string pFrom = $"@p{pIndex++}";
                string pTo = $"@p{pIndex++}";

                parameters.Add(fromUtc);
                parameters.Add(toUtc);

                unionParts.Add($@"
                    SELECT 
                        t.floorplan_masked_area_id AS FloorplanMaskedAreaId,
                        a.restricted_status        AS RestrictedStatus
                    FROM [dbo].[{table}] t
                    LEFT JOIN floorplan_masked_area a 
                        ON a.id = t.floorplan_masked_area_id
                    WHERE t.trans_time >= {pFrom}
                    AND t.trans_time <= {pTo}
                ");
            }

            var fullSql = string.Join("\nUNION ALL\n", unionParts);

            var raw = await _context.Database
                .SqlQueryRaw<AccessRaw>(fullSql, parameters.ToArray())
                .ToListAsync();

            // distinct area
            var uniqueAreas = raw
                .Where(x => x.FloorplanMaskedAreaId.HasValue)
                .GroupBy(x => x.FloorplanMaskedAreaId)
                .Select(g => g.First())
                .ToList();

            return new TrackingAccessPermissionSummaryRM
            {
                AccessedAreaTotal = uniqueAreas.Count,
                WithPermission = uniqueAreas.Count(x => x.RestrictedStatus == "non-restrict"),
                WithoutPermission = uniqueAreas.Count(x => x.RestrictedStatus == "restrict")
            };
        }

        public class AccessRaw
        {
            public Guid? FloorplanMaskedAreaId { get; set; }
            public string RestrictedStatus { get; set; }
        }







        // daily sum
        public async Task<List<TrackingDailySummaryRM>> GetDailySummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-7);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;
            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            query = ApplyFilters(query, request);

            var incidents = await query
                .Select(t => new
                {
                    Date = t.TransTime.Value.Date,
                    t.CardId
                })
                .Distinct()
                .ToListAsync();

            var grouped = incidents
                .GroupBy(x => x.Date)
                .Select(g => new TrackingDailySummaryRM
                {
                    Date = g.Key,
                    TotalRecords = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }


        // building sum
        public async Task<List<TrackingBuildingSummaryRM>> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);
            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Include(t => t.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            query = ApplyFilters(query, request);

            var data = await query
                .Select(t => new
                {
                    t.CardId,
                    BuildingId = t.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
                    BuildingName = t.FloorplanMaskedArea.Floorplan.Floor.Building.Name
                })
                .Distinct()
                .GroupBy(x => new { x.BuildingId, x.BuildingName })
                .Select(g => new TrackingBuildingSummaryRM
                {
                    BuildingId = g.Key.BuildingId,
                    BuildingName = g.Key.BuildingName,
                    TotalRecords = g.Count()
                })
                .OrderByDescending(x => x.TotalRecords)
                .ToListAsync();

            return data;
        }


        // visitor sum
        public async Task<List<TrackingVisitorSummaryRM>> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);
            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Include(t => t.Card.Visitor)
                .Include(t => t.Card.Member)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            query = ApplyFilters(query, request);

            var incidents = await query
                .Select(t => new
                {
                    t.CardId,
                    t.Card.VisitorId,
                    VisitorName = t.Card.Visitor != null ? t.Card.Visitor.Name : null,
                    t.Card.MemberId,
                    MemberName = t.Card.Member != null ? t.Card.Member.Name : null
                })
                .Distinct()
                .ToListAsync();

            var grouped = incidents
                .GroupBy(x => new { x.VisitorId, x.VisitorName, x.MemberId, x.MemberName })
                .Select(g => new TrackingVisitorSummaryRM
                {
                    VisitorId = g.Key.VisitorId,
                    VisitorName = g.Key.VisitorName,
                    MemberId = g.Key.MemberId,
                    MemberName = g.Key.MemberName,
                    TotalRecords = g.Count()
                })
                .OrderByDescending(x => x.TotalRecords)
                .ToList();

            return grouped;
        }


        // card sum - tracking-sum - latest tracking
        public async Task<List<TrackingCardSummaryRM>> GetCardSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var (from, to) = (
                request.From ?? DateTime.UtcNow.AddDays(-7),
                request.To ?? DateTime.UtcNow
            );

            var tableName = GetTableNameByDate(DateTime.UtcNow);

            // Query dasar
            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                // .Include(t => t.Card)
                //     .ThenInclude(c => c.Visitor)
                // .Include(t => t.Card)
                //     .ThenInclude(c => c.Member)
                // .Include(t => t.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            // filter opsional
            query = ApplyFilters(query, request);

            // Ambil field yang dibutuhkan
            var raw = await query
                .Select(t => new
                {
                    IdentityId = t.Visitor.IdentityId ?? t.Member.IdentityId,
                    t.CardId,
                    CardName = t.Card.Name,
                    t.Card.CardNumber,
                    t.Card.VisitorId,
                    VisitorName = t.Card.Visitor != null ? t.Card.Visitor.Name : null,
                    t.Card.MemberId,
                    MemberName = t.Card.Member != null ? t.Card.Member.Name : null,
                    t.TransTime,
                    t.CoordinateX,
                    t.CoordinateY,
                    AreaId = t.FloorplanMaskedArea.Id,
                    AreaName = t.FloorplanMaskedArea.Name,
                    FloorplanId = t.FloorplanMaskedArea.Floorplan.Id,
                    FloorplanName = t.FloorplanMaskedArea.Floorplan.Name,
                    FloorId = t.FloorplanMaskedArea.Floorplan.Floor.Id,
                    FloorName = t.FloorplanMaskedArea.Floorplan.Floor.Name,
                    BuildingId = t.FloorplanMaskedArea.Floorplan.Floor.Building.Id,
                    BuildingName = t.FloorplanMaskedArea.Floorplan.Floor.Building.Name,
                    t.FloorplanMaskedArea.Floorplan.FloorplanImage,
                })
                .ToListAsync();

            if (raw.Count == 0)
                return new List<TrackingCardSummaryRM>();

            // Group per Card
            var grouped = raw
                .GroupBy(x => new
                {
                    x.IdentityId,
                    x.CardId,
                    x.CardName,
                    x.CardNumber,
                    x.VisitorId,
                    x.VisitorName,
                    x.MemberId,
                    x.MemberName
                })
                .Select(g =>
                {
                    var first = g.OrderBy(x => x.TransTime).First();
                    var last = g.OrderByDescending(x => x.TransTime).First(); // posisi terakhir

                    return new TrackingCardSummaryRM
                    {
                        IdentityId = g.Key.IdentityId,
                        CardId = g.Key.CardId,
                        CardName = g.Key.CardName,
                        CardNumber = g.Key.CardNumber,
                        VisitorId = g.Key.VisitorId,
                        VisitorName = g.Key.VisitorName,
                        MemberId = g.Key.MemberId,
                        MemberName = g.Key.MemberName,
                        // TotalRecords = g.Count(),
                        EnterTime = first.TransTime,
                        LastDetectedAt = last.TransTime,

                        // Lokasi terakhir
                        BuildingId = last.BuildingId,
                        BuildingName = last.BuildingName,
                        FloorId = last.FloorId,
                        FloorName = last.FloorName,
                        FloorplanId = last.FloorplanId,
                        FloorplanName = last.FloorplanName,
                        FloorplanImage = last.FloorplanImage,
                        AreaId = last.AreaId,
                        AreaName = last.AreaName,
                        LastX = (float)Math.Round(last.CoordinateX ?? 0),
                        LastY = (float)Math.Round(last.CoordinateY ?? 0)
                    };
                })
                .OrderByDescending(x => x.LastDetectedAt)
                .ToList();

            return grouped;
        }



        // card sum
        //        public async Task<List<TrackingCardSummaryRM>> GetCardSummaryAsync(TrackingAnalyticsRequestRM request)
        // {
        //     var (from, to) = (
        //         request.From ?? DateTime.UtcNow.AddDays(-7),
        //         request.To ?? DateTime.UtcNow
        //     );

        //     var tableName = GetTableNameByDate(DateTime.UtcNow); // WIB-based table name

        //     var query = _context.Set<TrackingTransaction>()
        //         .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
        //         .AsNoTracking()
        //         .Include(t => t.Card)
        //             .ThenInclude(c => c.Visitor)
        //         .Include(t => t.Card)
        //             .ThenInclude(c => c.Member)
        //         .Where(t => t.TransTime >= from && t.TransTime <= to);

        //     // Apply optional filters (building, floor, area, etc.)
        //     query = ApplyFilters(query, request);

        //     // 🔹 Ambil hanya field yang dibutuhkan untuk efisiensi
        //     var raw = await query
        //         .Select(t => new
        //         {
        //             t.CardId,
        //             CardName = t.Card.Name,
        //             t.Card.VisitorId,
        //             VisitorName = t.Card.Visitor != null ? t.Card.Visitor.Name : null,
        //             t.Card.MemberId,
        //             MemberName = t.Card.Member != null ? t.Card.Member.Name : null,
        //             t.TransTime
        //         })
        //         .ToListAsync();

        //     if (raw.Count == 0)
        //         return new List<TrackingCardSummaryRM>();

        //     // 🔹 DISTINCT Card agar 1 Card dihitung 1x per visitor/member (hapus spam MQTT)
        //     var grouped = raw
        //         .GroupBy(x => new
        //         {
        //             x.CardId,
        //             x.CardName,
        //             x.VisitorId,
        //             x.VisitorName,
        //             x.MemberId,
        //             x.MemberName
        //         })
        //         .Select(g => new TrackingCardSummaryRM
        //         {
        //             CardId = g.Key.CardId,
        //             CardName = g.Key.CardName,
        //             VisitorId = g.Key.VisitorId,
        //             VisitorName = g.Key.VisitorName,
        //             MemberId = g.Key.MemberId,
        //             MemberName = g.Key.MemberName,
        //             TotalRecords = g.Count(),
        //             EnterTime = g.Min(x => x.TransTime), // waktu pertama muncul
        //             ExitTime = g.Max(x => x.TransTime)   // waktu terakhir muncul
        //         })
        //         .OrderByDescending(x => x.TotalRecords)
        //         .ToList();

        //     return grouped;
        // }


        //         // ===========================================================
        //         // 5️⃣ Reader Summary
        //         // ===========================================================
        public async Task<List<TrackingReaderSummaryRM>> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var (from, to) = (request.From ?? DateTime.UtcNow.AddDays(-7), request.To ?? DateTime.UtcNow);
            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Include(t => t.Reader)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            query = ApplyFilters(query, request);

            var data = await query
                .Select(t => new
                {
                    t.ReaderId,
                    ReaderName = t.Reader.Name,
                    t.CardId
                })
                .Distinct()
                .GroupBy(x => new { x.ReaderId, x.ReaderName })
                .Select(g => new TrackingReaderSummaryRM
                {
                    ReaderId = g.Key.ReaderId,
                    ReaderName = g.Key.ReaderName,
                    TotalRecords = g.Count() // Total card unik yang pernah dibaca reader itu
                })
                .OrderByDescending(x => x.TotalRecords)
                .ToListAsync();

            return data;
        }


        // public async Task<List<TrackingMovementRM>> GetTrackingMovementByCardIdAsync(Guid cardId)
        // {
        //     var tableName = GetTableNameByDate(DateTime.UtcNow); // WIB-based table name

        //     var query = _context.Set<TrackingTransaction>()
        //         .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE @card", cardId)
        //         .AsNoTracking()
        //         .Include(t => t.Card)
        //             .ThenInclude(c => c.Visitor)
        //         .Include(t => t.Card)
        //             .ThenInclude(c => c.Member)
        //         .Include(t => t.FloorplanMaskedArea.Floorplan.Floor.Building)
        //         .OrderBy(t => t.TransTime);

        //     var list = await query.ToListAsync();

        //     if (list.Count == 0)
        //         return new List<TrackingMovementRM>();

        //     // Group by area, karena 1 Card bisa pindah area dalam hari yang sama
        //     var grouped = list
        //         .GroupBy(t => t.FloorplanMaskedAreaId)
        //         .Select(g =>
        //         {
        //             var first = g.First();
        //             var last = g.Last();

        //             // Tentukan nama berdasarkan apakah Card milik Visitor atau Member
        //             var card = first.Card;
        //             var name =
        //                 card?.Visitor?.Name ??
        //                 card?.Member?.Name ??
        //                 "Unknown";

        //             return new TrackingMovementRM
        //             {
        //                 CardId = cardId,
        //                 PersonName = name,
        //                 Building = first.FloorplanMaskedArea?.Floorplan?.Floor?.Building?.Name ?? "-",
        //                 Floor = first.FloorplanMaskedArea?.Floorplan?.Floor?.Name ?? "-",
        //                 Floorplan = first.FloorplanMaskedArea?.Floorplan?.Name ?? "-",
        //                 Area = first.FloorplanMaskedArea?.Name ?? "-",
        //                 EnterTime = first.TransTime,
        //                 ExitTime = last.TransTime,
        //                 Positions = g.Select(p => new TrackingPositionPointRM
        //                 {
        //                     X = (float)Math.Round(p.CoordinateX ?? 0),
        //                     Y = (float)Math.Round(p.CoordinateY ?? 0)
        //                 })
        //                 .Distinct()
        //                 .ToList()
        //             };
        //         })
        //         .OrderBy(x => x.EnterTime)
        //         .ToList();

        //     return grouped;
        // }




        public async Task<List<TrackingMovementRM>> GetTrackingMovementByCardIdAsync(Guid cardId)
        {
            var tableName = GetTableNameByDate(DateTime.UtcNow);
            var connectionString = _context.Database.GetConnectionString();

            using var connection = new SqlConnection(connectionString);

            try
            {
                // Query tunggal dengan semua JOIN untuk mendapatkan semua data sekaligus
                var sql = $@"
            WITH TrackingData AS (
                SELECT 
                    t.trans_time,
                    t.floorplan_masked_area_id,
                    t.coordinate_x,
                    t.coordinate_y,
                    t.card_id,
                    t.visitor_id,
                    t.member_id
                FROM [dbo].[{tableName}] t
                WHERE t.card_id = @cardId
            )
            SELECT 
                -- Tracking data
                td.trans_time AS TransTime,
                td.floorplan_masked_area_id AS AreaId,
                td.coordinate_x AS CoordinateX,
                td.coordinate_y AS CoordinateY,
                
                -- Person Name Logic (SAMA PERSIS dengan EF Core version)
                COALESCE(
                    -- Priority 1: Visitor name from Card
                    cv.name,
                    -- Priority 2: Member name from Card  
                    cm.name,
                    -- Priority 3: Direct visitor name
                    dv.name,
                    -- Priority 4: Direct member name
                    dm.name,
                    'Unknown'
                ) AS PersonName,
                
                -- Area Hierarchy
                a.name AS AreaName,
                fp.name AS FloorplanName,
                f.name AS FloorName,
                b.name AS BuildingName
                
            FROM TrackingData td
            
            -- Untuk mendapatkan nama person melalui Card (Priority 1 & 2)
            LEFT JOIN card c ON td.card_id = c.id
            LEFT JOIN visitor cv ON c.visitor_id = cv.id
            LEFT JOIN mst_member cm ON c.member_id = cm.id AND cm.status <> 0
            
            -- Untuk mendapatkan nama person langsung (Priority 3 & 4)
            LEFT JOIN visitor dv ON td.visitor_id = dv.id
            LEFT JOIN mst_member dm ON td.member_id = dm.id AND dm.status <> 0
            
            -- Untuk mendapatkan area hierarchy
            LEFT JOIN floorplan_masked_area a ON td.floorplan_masked_area_id = a.id
            LEFT JOIN mst_floorplan fp ON a.floorplan_id = fp.id
            LEFT JOIN mst_floor f ON fp.floor_id = f.id
            LEFT JOIN mst_building b ON f.building_id = b.id
            
            ORDER BY td.trans_time";

                var results = (await connection.QueryAsync<TrackingFullData>(sql, new { cardId }))
                    .ToList();

                if (!results.Any())
                    return new List<TrackingMovementRM>();

                // Group by area ID - SAMA PERSIS dengan logika EF Core
                var grouped = results
                    .GroupBy(t => t.AreaId)
                    .Select(g =>
                    {
                        var first = g.First();
                        var last = g.Last();

                        // Buat list positions dengan Distinct() - SAMA dengan EF Core
                        var positions = g
                            .Where(p => p.CoordinateX.HasValue && p.CoordinateY.HasValue)
                            .Select(p => new TrackingPositionPointRM
                            {
                                X = (float)Math.Round(p.CoordinateX.Value, 0),
                                Y = (float)Math.Round(p.CoordinateY.Value, 0)
                            })
                            .Distinct()
                            .ToList();

                        return new TrackingMovementRM
                        {
                            CardId = cardId,
                            PersonName = first.PersonName ?? "Unknown",
                            Building = first.BuildingName ?? "-",
                            Floor = first.FloorName ?? "-",
                            Floorplan = first.FloorplanName ?? "-",
                            Area = first.AreaName ?? "-",
                            EnterTime = first.TransTime,
                            ExitTime = last.TransTime,
                            Positions = positions
                        };
                    })
                    .OrderBy(x => x.EnterTime)
                    .ToList();

                return grouped;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error in GetTrackingMovementByCardIdAsync with Dapper for card {CardId}", cardId);
                throw;
            }
        }

        // DTO untuk mapping hasil query
        public class TrackingFullData
        {
            public DateTime? TransTime { get; set; }
            public Guid? AreaId { get; set; }
            public float? CoordinateX { get; set; }
            public float? CoordinateY { get; set; }
            public string PersonName { get; set; }
            public string AreaName { get; set; }
            public string FloorplanName { get; set; }
            public string FloorName { get; set; }
            public string BuildingName { get; set; }
        }

        public async Task<List<TrackingHeatmapRM>> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request)
        {
            var tableName = GetTableNameByDate(DateTime.UtcNow);
            var (from, to) = (request.From ?? DateTime.UtcNow.AddHours(-2), request.To ?? DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Where(t => t.TransTime >= from && t.TransTime <= to)
                .Where(t => t.CoordinateX != null && t.CoordinateY != null);

            // filter opsional
            if (request.FloorplanId.HasValue)
                query = query.Where(t => t.FloorplanMaskedArea.Floorplan.Id == request.FloorplanId);

            if (request.AreaId.HasValue)
                query = query.Where(t => t.FloorplanMaskedAreaId == request.AreaId);

            // ambil hanya X/Y + Floorplan
            var points = await query
                .Select(t => new
                {
                    X = t.CoordinateX!.Value,
                    Y = t.CoordinateY!.Value,
                    FloorplanId = t.FloorplanMaskedArea.Floorplan.Id,
                    MaskedAreaId = t.FloorplanMaskedAreaId
                })
                .ToListAsync();

            // group & count (agar jadi intensitas)
            var grouped = points
                .GroupBy(p => new { p.FloorplanId, p.MaskedAreaId, p.X, p.Y })
                .Select(g => new TrackingHeatmapRM
                {
                    FloorplanId = g.Key.FloorplanId,
                    MaskedAreaId = g.Key.MaskedAreaId,
                    X = g.Key.X,
                    Y = g.Key.Y,
                    Count = g.Count()
                })
                .ToList();

            return grouped;
        }

        public async Task<TrackingHierarchyRM> GetHierarchySummaryAsync(TrackingAnalyticsRequestRM request)
        {
            var range = GetTimeRange(request.TimeRange);
            var from = range?.from ?? request.From ?? DateTime.UtcNow.AddDays(-1);
            var to = range?.to ?? request.To ?? DateTime.UtcNow;

            var tableName = GetTableNameByDate(DateTime.UtcNow);

            var query = _context.Set<TrackingTransaction>()
                .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
                .AsNoTracking()
                .Include(t => t.FloorplanMaskedArea.Floorplan.Floor.Building)
                .Where(t => t.TransTime >= from && t.TransTime <= to);

            // Optional: filter only for detail mode
            query = ApplyFilters(query, request);

            var raw = await query
                .Select(t => new
                {
                    BuildingId = t.FloorplanMaskedArea.Floorplan.Floor.Building.Id.ToString(),
                    BuildingName = t.FloorplanMaskedArea.Floorplan.Floor.Building.Name,
                    FloorId = t.FloorplanMaskedArea.Floorplan.Floor.Id.ToString(),
                    FloorName = t.FloorplanMaskedArea.Floorplan.Floor.Name,
                    FloorplanId = t.FloorplanMaskedArea.Floorplan.Id.ToString(),
                    FloorplanName = t.FloorplanMaskedArea.Floorplan.Name,
                    AreaId = t.FloorplanMaskedAreaId.ToString(),
                    AreaName = t.FloorplanMaskedArea.Name
                })
                .Distinct()
                .ToListAsync();

            var result = new TrackingHierarchyRM();

            foreach (var row in raw)
            {
                // ========= BUILDING =========
                if (!result.Buildings.TryGetValue(row.BuildingId, out var buildingNode))
                {
                    buildingNode = new TrackingBuildingNode
                    {
                        Id = row.BuildingId,
                        Name = row.BuildingName
                    };
                    result.Buildings[row.BuildingId] = buildingNode;
                }
                buildingNode.Count++;

                // ========= FLOOR =========
                if (!buildingNode.Floors.TryGetValue(row.FloorId, out var floorNode))
                {
                    floorNode = new TrackingFloorNode
                    {
                        Id = row.FloorId,
                        Name = row.FloorName
                    };
                    buildingNode.Floors[row.FloorId] = floorNode;
                }
                floorNode.Count++;

                // ========= FLOORPLAN =========
                if (!floorNode.Floorplans.TryGetValue(row.FloorplanId, out var floorplanNode))
                {
                    floorplanNode = new TrackingFloorplanNode
                    {
                        Id = row.FloorplanId,
                        Name = row.FloorplanName
                    };
                    floorNode.Floorplans[row.FloorplanId] = floorplanNode;
                }
                floorplanNode.Count++;

                // ========= AREA =========
                if (!floorplanNode.Areas.TryGetValue(row.AreaId, out var areaNode))
                {
                    areaNode = new TrackingAreaNode
                    {
                        Id = row.AreaId,
                        Name = row.AreaName
                    };
                    floorplanNode.Areas[row.AreaId] = areaNode;
                }
                areaNode.Count++;
            }

            return result;
        }


        // helpers filter
        private IQueryable<TrackingTransaction> ApplyFilters(IQueryable<TrackingTransaction> query, TrackingAnalyticsRequestRM request)
        {
            if (request.BuildingId.HasValue)
                query = query.Where(a => a.FloorplanMaskedArea.Floorplan.Floor.Building.Id == request.BuildingId);

            if (request.FloorId.HasValue)
                query = query.Where(a => a.FloorplanMaskedArea.Floorplan.Floor.Id == request.FloorId);

            if (request.AreaId.HasValue)
                query = query.Where(a => a.FloorplanMaskedAreaId == request.AreaId);

            if (request.VisitorId.HasValue)
                query = query.Where(a => a.VisitorId == request.VisitorId);

            if (request.ReaderId.HasValue)
                query = query.Where(a => a.ReaderId == request.ReaderId);

            if (!string.IsNullOrEmpty(request.IdentityId))
            {
                var cardIds = _context.Set<Card>()
                    .Where(c =>
                        (c.Visitor != null && c.Visitor.IdentityId == request.IdentityId) ||
                        (c.Member != null && c.Member.IdentityId == request.IdentityId)
                    )
                    .Select(c => c.Id);

                query = query.Where(t =>
                    t.CardId.HasValue && cardIds.Contains(t.CardId.Value));
            }

            return query;
        }

        // === GET TABLE NAMES IN RANGE (WIB) ===
        private List<string> GetTableNamesInRange(DateTime fromUtc, DateTime toUtc)
        {
            var fromWib = TimeZoneInfo.ConvertTimeFromUtc(fromUtc, WibZone).Date;
            var toWib = TimeZoneInfo.ConvertTimeFromUtc(toUtc, WibZone).Date;

            var tables = new List<string>();
            for (var d = fromWib; d <= toWib; d = d.AddDays(1))
            {
                // tables.Add($"tracking_transaction_{d:yyyyMMdd}");
                var table = $"tracking_transaction_{d:yyyyMMdd}";
                if (TableExists(table))
                    tables.Add(table);
            }
            return tables;
        }

        private bool TableExists(string tableName)
        {
            var connString = _context.Database.GetConnectionString();

            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_SCHEMA = 'dbo'
                    AND TABLE_NAME = @name
                ";

            cmd.Parameters.AddWithValue("@name", tableName);

            var result = (int)cmd.ExecuteScalar();
            return result > 0;
        }


    }
}
