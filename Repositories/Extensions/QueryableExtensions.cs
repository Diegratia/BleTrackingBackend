using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Repositories.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortColumn, string sortDir)
        {
            if (string.IsNullOrEmpty(sortColumn))
            {
                // Default sorting if needed, or just return
               sortColumn = "UpdatedAt";
            }
            if (string.IsNullOrEmpty(sortDir) || !new[] { "asc", "desc" }.Contains(sortDir.ToLower()))
            {
                sortDir = "desc";
            }

            // Using System.Linq.Dynamic.Core
            return query.OrderBy($"{sortColumn} {sortDir}");
        }
    }
}
