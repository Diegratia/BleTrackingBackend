using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class PatrolRoute : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("status")]
        public int Status { get; set; } = 1;
        public MstApplication Application { get; set; }
        public ICollection<PatrolRouteAreas> PatrolRouteAreas { get; set; } = new List<PatrolRouteAreas>();
        public ICollection<PatrolRouteTimeGroups> PatrolRouteTimeGroups { get; set; } = new List<PatrolRouteTimeGroups>();
    }
}