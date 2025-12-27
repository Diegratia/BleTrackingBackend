using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
        public class CardAccessCreateDto : BaseModelDto
        {
            public string? Name { get; set; }
            public string? AccessScope { get; set; }
            public string? Remarks { get; set; }

            public List<Guid?> MaskedAreaIds { get; set; } = new();
            public List<Guid?> TimeGroupIds { get; set; } = new();

            // 🔹 Tambahan opsional
            public Guid? BuildingId { get; set; }
            public Guid? FloorId { get; set; }
            public Guid? FloorplanId { get; set; }
        }


    public class CardAccessUpdateDto
    {
        public string? Name { get; set; }
        public string? Remarks { get; set; }
        public string? AccessScope { get; set; }
        public List<Guid?> MaskedAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();

                    // 🔹 Tambahan opsional
            public Guid? BuildingId { get; set; }
            public Guid? FloorId { get; set; }
            public Guid? FloorplanId { get; set; }

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

    // public class CardAssignAccessDto : BaseModelDto
    // {
    //     public Guid CardId { get; set; }
    //     public List<Guid> CardAccessIds { get; set; } = new();
    // }

        public class    CardAssignAccessDto : BaseModelDto
    {
        public Guid CardId { get; set; }

        // Bisa kirim salah satu dari bawah ini
        public Guid? BuildingId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? FloorplanId { get; set; }

        // manual CardAccessIds (optional)
        public List<Guid>? CardAccessIds { get; set; } = new();
    }

    
    
}
