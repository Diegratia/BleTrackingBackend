using VM = Data.ViewModels;
using RM = Repositories.Models;

namespace BusinessLogic.Services.Extension.RootExtension
{
    public static class DateFilterMapper
    {
        public static Dictionary<string, RM.DateRangeFilter> ToRepositoryDateFilters(
            this Dictionary<string, VM.DateRangeFilter>? src)
        {
            if (src == null || src.Count == 0)
                return new Dictionary<string, RM.DateRangeFilter>();

            return src.ToDictionary(
                k => k.Key,
                v => new RM.DateRangeFilter
                {
                    DateFrom = v.Value.DateFrom,
                    DateTo = v.Value.DateTo
                });
        }
    }
}
