using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Helpers.Consumer
{
    public static class TimezoneHelper
        {
            public static TimeZoneInfo Resolve(string? timezone)
            {
                if (string.IsNullOrWhiteSpace(timezone))
                    return TimeZoneInfo.Utc;

                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(timezone);
                }
                catch
                {
                    return TimeZoneInfo.Utc;
                }
            }

            public static DateTime ConvertFromUtc(DateTime utc, TimeZoneInfo tz)
                => TimeZoneInfo.ConvertTimeFromUtc(utc, tz);

            public static DateTime? ConvertFromUtc(DateTime? utc, TimeZoneInfo tz)
                => utc.HasValue ? ConvertFromUtc(utc.Value, tz) : null;
        }
}