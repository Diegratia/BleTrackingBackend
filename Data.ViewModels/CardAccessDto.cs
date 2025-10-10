using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
        public class CardAccessCreateDto : BaseModelDto
    {
        public string? Name { get; set; }
        public string? AccessScope { get; set; }
        public string? Remarks { get; set; }
        public List<Guid?> MaskedAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
    }

    public class CardAccessUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AccessScope { get; set; }
        public List<Guid?> MaskedAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
        
    }

    public class CardAccessDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? AccessNumber { get; set; }
        public string? AccessScope { get; set; }
        public string? Remarks { get; set; }
        public List<Guid?> MaskedAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();

    }
}
