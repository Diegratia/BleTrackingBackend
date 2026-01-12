using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Extension
{
    public static class CollectionExtensions
    {
    public static void RemoveWhere<T>(
            this ICollection<T> source,
            Func<T, bool> predicate)
        {
            var items = source.Where(predicate).ToList();
            foreach (var item in items)
                source.Remove(item);
        }
    }
}