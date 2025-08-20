using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface
{
   public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> action,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken ct = default);

        Task ExecuteInTransactionAsync(
            Func<Task> action,
            IsolationLevel isolation = IsolationLevel.ReadCommitted,
            CancellationToken ct = default);
    }
}


// Kontrak

