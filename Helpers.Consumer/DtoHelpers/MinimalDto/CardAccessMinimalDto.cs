using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Helpers.Consumer.DtoHelpers.MinimalDto
{
    public class CardAccessMinimalDto : BaseModelDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int AccessNumber { get; set; }
        public string? AccessScope { get; set; }
        public string? Remarks { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Guid?> MaskedAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
    }
}