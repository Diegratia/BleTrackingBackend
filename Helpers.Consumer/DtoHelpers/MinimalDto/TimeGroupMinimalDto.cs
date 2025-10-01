using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Helpers.Consumer.DtoHelpers.MinimalDto
{
    public class TimeGroupMinimalDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<TimeBlockMinimalDto> TimeBlocks { get; set; } = new();
        public List<Guid?> CardAccessIds { get; set; } = new();
        public DateTime UpdatedAt { get; set; } 
    }
}

