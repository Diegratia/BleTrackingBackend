using System;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.JobsScheduler
{
    [DisallowConcurrentExecution]
    public class DropOldTrackingTablesJob : IJob
    {
        private readonly BleTrackingDbContext _context;
        private readonly ILogger<DropOldTrackingTablesJob> _logger;

        public DropOldTrackingTablesJob(BleTrackingDbContext context, ILogger<DropOldTrackingTablesJob> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var wibNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);
            var thresholdDate = wibNow.AddDays(-30).ToString("yyyyMMdd");

            _logger.LogInformation("🧹 Starting DropOldTrackingTablesJob at {TimeWIB} WIB (threshold: {Threshold})", wibNow.ToString("yyyy-MM-dd HH:mm:ss"), thresholdDate);

            try
            {
                // Step 1: Get list of tables to drop
                var getTablesSql = @"
                    SELECT name
                    FROM sys.tables
                    WHERE name LIKE 'tracking_transaction_%'
                    AND TRY_CAST(RIGHT(name, 8) AS INT) < @thresholdDate
                    ORDER BY name;";

                var thresholdParam = new Microsoft.Data.SqlClient.SqlParameter("@thresholdDate", thresholdDate);

                var tables = await _context.Database
                    .SqlQueryRaw<string>(getTablesSql, thresholdParam)
                    .ToListAsync();

                if (tables.Count == 0)
                {
                    _logger.LogInformation("✅ No old tracking tables found (older than {Threshold})", thresholdDate);
                    return;
                }

                _logger.LogInformation("Found {Count} tables to drop (older than {Threshold})", tables.Count, thresholdDate);
                _logger.LogInformation("Tables: {Tables}", string.Join(", ", tables));

                // Step 2: Drop each table individually
                var droppedCount = 0;
                var failedCount = 0;

                foreach (var table in tables)
                {
                    try
                    {
                        var dropSql = $"DROP TABLE [dbo].[{table}];";
                        await _context.Database.ExecuteSqlRawAsync(dropSql);
                        droppedCount++;
                        _logger.LogInformation("Dropped table: {TableName} ({Count}/{Total})", table, droppedCount, tables.Count);
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, "Failed to drop table: {TableName}", table);
                    }
                }

                _logger.LogInformation("✅ DropOldTrackingTablesJob completed at {TimeWIB} WIB - Dropped: {Dropped}, Failed: {Failed}",
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone).ToString("yyyy-MM-dd HH:mm:ss"),
                    droppedCount, failedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while executing DropOldTrackingTablesJob at {TimeWIB} WIB", wibNow);
                throw;
            }
        }
    }
}
