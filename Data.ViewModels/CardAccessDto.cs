// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Data.ViewModels
// {
//         public class CardAccessCreateDto
//     {
//         public string Name { get; set; }
//         public string AccessNumber { get; set; }
//         public string? Remarks { get; set; }
//         public Guid ApplicationId { get; set; }
//         public List<Guid> MaskedAreaIds { get; set; } = new();
//     }

//     public class CardAccessUpdateDto
//     {
//         public Guid Id { get; set; }
//         public string Name { get; set; }
//         public string AccessNumber { get; set; }
//         public string? Remarks { get; set; }
//         public List<Guid> MaskedAreaIds { get; set; } = new();
//     }

//     public class CardAccessDto
//     {
//         public Guid Id { get; set; }
//         public string Name { get; set; }
//         public string AccessNumber { get; set; }
//         public string? Remarks { get; set; }
//         public Guid ApplicationId { get; set; }
//         public List<FloorplanMaskedAreaDto> MaskedAreas { get; set; } = new();
//     }

// }
