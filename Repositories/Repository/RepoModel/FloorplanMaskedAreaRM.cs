using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Repositories.Repository.RepoModel
{
    public class MinimalMaskedAreaRM
    {
        public Guid Id { get; set; }
        public Guid FloorplanId { get; set; }
        public Guid FloorId { get; set; }
        public string Name { get; set; }
        public string AreaShape { get; set; }
        public string ColorArea { get; set; }
        public string RestrictedStatus { get; set; }
        public bool AllowFloorChange { get; set; } = false;
    }
    
            public class AreaSummaryRM
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }
}