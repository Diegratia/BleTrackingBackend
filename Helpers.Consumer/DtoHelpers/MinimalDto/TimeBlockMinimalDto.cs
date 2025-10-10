using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Helpers.Consumer.DtoHelpers.MinimalDto
{
    public class TimeBlockMinimalDto : BaseModelDto
    {
        public Guid Id { get; set;}
        public string? DayOfWeek { get; set; }  // pakai string
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public Guid? TimeGroupId { get; set; }
    }
}

