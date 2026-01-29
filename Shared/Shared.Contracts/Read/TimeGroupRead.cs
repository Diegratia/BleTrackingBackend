using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
     public class TimeBlockRead
        {
            public Guid Id { get; set;}
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public DayOfWeek? DayOfWeek { get; set; }  
            public TimeSpan? StartTime { get; set; }
            public TimeSpan? EndTime { get; set; }
        }
}