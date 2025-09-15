using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class TimeGroupDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<TimeBlockDto> TimeBlocks { get; set; } = new();
    }


    public class TimeGroupCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<TimeBlockCreateDto> TimeBlocks { get; set; } = new();
    }

    public class TimeGroupUpdateDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }
        public List<TimeBlockUpdateDto> TimeBlocks { get; set; } = new();
    }
}