using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Repositories.DbContexts;

namespace BusinessLogic.Services.JobsScheduler
{
    public class CreateDailyTrackingTableJob : IJob
    {
        private readonly BleTrackingDbContext _context;
        private readonly ILogger<CreateDailyTrackingTableJob> _logger;

        public CreateDailyTrackingTableJob(BleTrackingDbContext context, ILogger<CreateDailyTrackingTableJob> logger)
        {
            _context = context;
            _logger = logger;
        }
        

        public async Task Execute(IJobExecutionContext context)
        {
            // var wibNow = DateTime.UtcNow.AddHours(7);
            var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var wibNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);
            var tableName = $"tracking_transaction_{wibNow:yyyyMMdd}";

            try
            {
                _logger.LogInformation("Starting CreateDailyTrackingTableJob at {Time:UTC}", DateTime.UtcNow);
                await _context.Database.OpenConnectionAsync();
                _logger.LogInformation("Database connection successful at {Time:UTC}", DateTime.UtcNow);

                var sql = $@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{tableName}' AND xtype='U')
                    CREATE TABLE [dbo].[{tableName}] (
                        [id] [uniqueidentifier] NOT NULL,
                        [trans_time] [datetime2](7) NULL,
                        [reader_id] [uniqueidentifier] NULL,
                        [card_id] [uniqueidentifier] NULL,
                        [visitor_id] [uniqueidentifier] NULL,
                        [member_id] [uniqueidentifier] NULL,
                        [floorplan_masked_area_id] [uniqueidentifier] NULL,
                        [coordinate_x] [real] NULL,
                        [coordinate_y] [real] NULL,
                        [coordinate_px_y] [real] NULL,
                        [coordinate_px_x] [real] NULL,
                        [alarm_status] [nvarchar](255) NULL,
                        [battery] [bigint] NULL,
                        [application_id] [uniqueidentifier] NOT NULL,
                        CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ([id] ASC)
                    )
                        CREATE NONCLUSTERED INDEX [IX_{tableName}_trans_time] ON [dbo].[{tableName}] ([trans_time] ASC);
                        CREATE NONCLUSTERED INDEX [IX_{tableName}_application_id] ON [dbo].[{tableName}] ([application_id] ASC);
                        CREATE NONCLUSTERED INDEX [IX_{tableName}_reader_id] ON [dbo].[{tableName}] ([reader_id] ASC);
                        CREATE NONCLUSTERED INDEX [IX_{tableName}_visitor_id] ON [dbo].[{tableName}] ([visitor_id] ASC);
                    ";

                await _context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation("Table {TableName} created successfully at {Time:UTC}", tableName, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create table {TableName} at {Time:UTC}", tableName, DateTime.UtcNow);
                throw;
            }
        }
    }
}