using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
  [Table("patrol_route_time_groups")]
  public class PatrolRouteTimeGroups : IApplicationEntity
  {
    [Column("patrol_route_id")]
    public Guid PatrolRouteId { get; set; }
    public PatrolRoute? PatrolRoutes { get; set; }

    [Column("time_group_id")]
    public Guid TimeGroupId { get; set; }
    public TimeGroup? TimeGroup { get; set; } 

    [Column("application_id")]
    public Guid ApplicationId { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;
    
  }
}