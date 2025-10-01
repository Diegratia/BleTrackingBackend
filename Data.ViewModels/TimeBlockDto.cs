using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class TimeBlockDto : BaseModelDto
    {
        public Guid Id { get; set;}
        public string? DayOfWeek { get; set; }  // pakai string
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public Guid? TimeGroupId { get; set; }
    }

    public class TimeBlockCreateDto : BaseModelDto
    {
        public string? DayOfWeek { get; set; }  // pakai string
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public Guid? TimeGroupId { get; set; }
    }

    public class TimeBlockUpdateDto
    {
        public Guid? Id { get; set; }
        public string? DayOfWeek { get; set; }  // pakai string
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public Guid? TimeGroupId { get; set; }
    }
}
