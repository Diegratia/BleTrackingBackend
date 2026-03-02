using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Extension.Filters
{
        public static class FilterBinder
    {
        public static T Bind<T>(
            Dictionary<string, object> filters
        ) where T : new()
        {
            var result = new T();

            if (filters == null || filters.Count == 0)
                return result;

            // Serialize dictionary → JSON
            var json = JsonSerializer.Serialize(filters);

            try
            {
                var parsed = JsonSerializer.Deserialize<T>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters =
                        {
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                    });

                return parsed ?? result;
            }
            catch
            {
                return result; // fallback aman
            }
        }
    }

}