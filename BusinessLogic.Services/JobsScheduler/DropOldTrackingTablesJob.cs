using System;
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

            _logger.LogInformation("🧹 Starting DropOldTrackingTablesJob at {TimeWIB} WIB", wibNow);

            var sql = $@"
                DECLARE @sql NVARCHAR(MAX) = '';
                SELECT @sql = STRING_AGG('DROP TABLE [dbo].[' + name + '];', CHAR(13))
                FROM sys.tables
                WHERE name LIKE 'tracking_transaction_%'
                AND TRY_CAST(RIGHT(name, 8) AS INT) < @thresholdDate;

                IF @sql IS NOT NULL
                BEGIN
                    PRINT 'Dropping old tracking tables...';
                    EXEC sp_executesql @sql;
                END
                ELSE
                BEGIN
                    PRINT 'No old tracking tables found to drop.';
                END";

            var parameter = new Microsoft.Data.SqlClient.SqlParameter("@thresholdDate", thresholdDate);

            try
            {
                var affected = await _context.Database.ExecuteSqlRawAsync(sql, parameter);
                _logger.LogInformation("✅ DropOldTrackingTablesJob executed successfully at {TimeWIB} WIB", wibNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while executing DropOldTrackingTablesJob at {TimeWIB} WIB", wibNow);
                throw;
            }
        }
    }
}
