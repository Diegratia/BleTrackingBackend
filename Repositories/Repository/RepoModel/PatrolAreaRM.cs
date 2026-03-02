using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Repositories.Repository.RepoModel
{
    public class PatrolAreaRM
    {
        public Guid Id { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string? Name { get; set; }
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
    }

    public class PatrolAreaLookUpRM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? FloorName { get; set; }
        public string? FloorplanName { get; set; }
        public int? IsActive { get; set; }
    }
}