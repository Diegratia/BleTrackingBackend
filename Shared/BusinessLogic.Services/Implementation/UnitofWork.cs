using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace BusinessLogic.Services.Implementation
{
  // Implementasi (EF Core)
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly BleTrackingDbContext _db;
        public UnitOfWork(BleTrackingDbContext db) => _db = db;

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken ct = default)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(isolation, ct);
                try
                {
                    var result = await action();
                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);
                    return result;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task ExecuteInTransactionAsync(
            Func<Task> action,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken ct = default)
            => await ExecuteInTransactionAsync(async () => { await action(); return 0; }, isolation, ct);
    }

}