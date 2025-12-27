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
                var today = DateTime.UtcNow.ToString("yyyyMMdd"); // UTC
                var tableName = $"tracking_transaction_{today}";
            try
            {
                _logger.LogInformation("Starting CreateDailyTrackingTableJob at {Time:UTC}", DateTime.UtcNow);
                await _context.Database.OpenConnectionAsync();
                _logger.LogInformation("Database connection successful at {Time:UTC}", DateTime.UtcNow);

                var sql = $@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{tableName}' AND xtype='U')
                    CREATE TABLE [dbo].[{tableName}] (
                        [Id] [uniqueidentifier] NOT NULL,
                        [CreatedAt] [datetime2](7) NOT NULL,
                        [UpdatedAt] [datetime2](7) NULL,
                        [TransTime] [datetime2](7) NULL,
                        [ReaderId] [uniqueidentifier] NULL,
                        [CardId] [uniqueidentifier] NULL,
                        [VisitorId] [uniqueidentifier] NULL,
                        [MemberId] [uniqueidentifier] NULL,
                        [FloorplanMaskedAreaId] [uniqueidentifier] NULL,
                        [CoordinateX] [real] NULL,
                        [CoordinateY] [real] NULL,
                        [CoordinatePxX] [real] NULL,
                        [CoordinatePxY] [real] NULL,
                        [AlarmStatus] [nvarchar](255) NULL,
                        [Battery] [bigint] NULL,
                        [ApplicationId] [uniqueidentifier] NOT NULL,
                        CONSTRAINT [PK_{tableName}] PRIMARY KEY CLUSTERED ([Id] ASC)
                    )
                    CREATE NONCLUSTERED INDEX [IX_{tableName}_TransTime] ON [dbo].[{tableName}] ([TransTime] ASC)
                    CREATE NONCLUSTERED INDEX [IX_{tableName}_ApplicationId] ON [dbo].[{tableName}] ([ApplicationId] ASC)
                    CREATE NONCLUSTERED INDEX [IX_{tableName}_ReaderId] ON [dbo].[{tableName}] ([ReaderId] ASC)
                    CREATE NONCLUSTERED INDEX [IX_{tableName}_VisitorId] ON [dbo].[{tableName}] ([VisitorId] ASC)";

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