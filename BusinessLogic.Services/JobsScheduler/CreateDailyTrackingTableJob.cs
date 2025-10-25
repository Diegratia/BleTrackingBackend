using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.DbContexts;
using Quartz;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.JobsScheduler;

    public class CreateDailyTrackingTableJob : IJob
{
    private readonly BleTrackingDbContext _context;
    
    public CreateDailyTrackingTableJob(BleTrackingDbContext context)
    {
        _context = context;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateTime.Today.ToString("yyyyMMdd");
        var tableName = $"tracking_transaction_{today}";
        
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
    }
}
