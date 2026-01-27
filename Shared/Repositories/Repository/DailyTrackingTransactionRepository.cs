using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.DbContexts;
using Microsoft.AspNetCore.Http;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Repository.RepoModel;
using AutoMapper.QueryableExtensions;
using AutoMapper;

namespace Repositories.Repository;

    public class DailyTrackingTransactionRepository : BaseRepository
{
    private readonly BleTrackingDbContext _context;
     private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public DailyTrackingTransactionRepository(BleTrackingDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        : base(context, httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentTableName()
    {
        return $"tracking_transaction_{DateTime.Today:yyyyMMdd}";
    }

    public async Task AddAsync(TrackingTransaction transaction)
    {
        var tableName = GetCurrentTableName();
        var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

        if (!isSystemAdmin && applicationId.HasValue)
            transaction.ApplicationId = applicationId.Value;

        var sql = $@"
            INSERT INTO [dbo].[{tableName}] 
            (Id, CreatedAt, TransTime, ReaderId, CardId, VisitorId, MemberId, 
             FloorplanMaskedAreaId, CoordinateX, CoordinateY, CoordinatePxX, 
             CoordinatePxY, AlarmStatus, Battery, ApplicationId)
            VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14)";

        await _context.Database.ExecuteSqlRawAsync(sql, 
            transaction.Id, DateTime.UtcNow, transaction.TransTime, transaction.ReaderId,
            transaction.CardId, transaction.VisitorId, transaction.MemberId,
            transaction.FloorplanMaskedAreaId, transaction.CoordinateX, transaction.CoordinateY,
            transaction.CoordinatePxX, transaction.CoordinatePxY,
            transaction.AlarmStatus?.ToString(), transaction.Battery, transaction.ApplicationId);
    }

    public IQueryable<TrackingTransactionRM> GetTodayQueryable(DateTime? from = null, DateTime? to = null)
    {
        var tableName = GetCurrentTableName();
        
        var query = _context.Set<TrackingTransaction>()
            .FromSqlRaw($"SELECT * FROM [dbo].[{tableName}] WHERE 1=1")
            .AsNoTracking();

        if (from.HasValue) query = query.Where(t => t.TransTime >= from);
        if (to.HasValue) query = query.Where(t => t.TransTime <= to);

        return query.ProjectTo<TrackingTransactionRM>(_mapper.ConfigurationProvider);
    }
}
