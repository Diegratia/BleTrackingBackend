using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class TimeBlockRead
    {
        public Guid Id { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }

    public class TimeGroupRead : BaseRead
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduleType? ScheduleType { get; set; }
        public List<TimeBlockRead> TimeBlocks { get; set; } = new();
        public List<Guid?> CardAccessIds { get; set; } = new();
    }
}