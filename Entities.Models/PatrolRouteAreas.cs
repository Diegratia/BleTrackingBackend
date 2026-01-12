using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{
    [Index(nameof(PatrolRouteId))]
    [Index(nameof(PatrolAreaId))]
    public class PatrolRouteAreas : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column ("patrol_route_id")]
        public Guid PatrolRouteId { get; set; }
        [Column ("patrol_area_id")]
        public Guid PatrolAreaId { get; set; }
        [Column("order_index")]
        public int OrderIndex { get; set; }
        [Column("estimated_distance")]
        public float EstimatedDistance { get; set; }
        [Column("estimated_time")]
        public int EstimatedTime  { get; set; }
        [Column("start_area_id")]
        public Guid? StartAreaId { get; set; }
        [Column("end_area_id")]
        public Guid? EndAreaId { get; set; }
        [Column("status")]
        public int status { get; set; } = 1;
        public PatrolRoute PatrolRoute { get; set; }
        public PatrolArea PatrolArea { get; set; }
        public MstApplication Application { get; set; }
    } 
}