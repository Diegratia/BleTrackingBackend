using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.DbContexts;
using Quartz;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.JobsScheduler;

public class DropOldTrackingTablesJob : IJob
{
    private readonly BleTrackingDbContext _context;
    
    public DropOldTrackingTablesJob(BleTrackingDbContext context)
    {
        _context = context;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var thirtyDaysAgo = DateTime.Today.AddDays(-30).ToString("yyyyMMdd");
        
        var sql = $@"
            DECLARE @sql NVARCHAR(MAX) = '';
            SELECT @sql = @sql + 'DROP TABLE [dbo].[' + name + '];' + CHAR(13)
            FROM sys.tables 
            WHERE name LIKE 'tracking_transaction_%' 
            AND CAST(SUBSTRING(name, 23, 8) AS INT) < {thirtyDaysAgo};
            
            EXEC sp_executesql @sql";

        await _context.Database.ExecuteSqlRawAsync(sql);
    }
}