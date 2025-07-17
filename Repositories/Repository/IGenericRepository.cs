using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repositories.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<(IEnumerable<T>, int)> GetFilteredAsync(
            Expression<Func<T, bool>> filter = null,
            int page = 1,
            int pageSize = 10,
            Expression<Func<T, object>> orderBy = null,
            bool orderByDescending = false);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task SoftDeleteAsync(Guid id);
    }
}