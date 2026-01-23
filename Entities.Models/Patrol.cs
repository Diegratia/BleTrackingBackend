using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{
    public class PatrolArea : BaseModelWithTime, IApplicationEntity
    {

        [Column("name")]
        public string? Name { get; set; }

        [Column("area_shape")]
        public string? AreaShape { get; set; }
        [Column("color")]
        public string? Color { get; set; }
        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("floorplan_id")]
        public Guid? FloorplanId { get; set; }

        [Column("floor_id")]
        public Guid? FloorId { get; set; }

        [Column("is_active")]
        public int IsActive { get; set; } = 1;

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("application_id")]
        public Guid ApplicationId { get; set; }
        public MstApplication Application { get; set; }
        public MstFloor Floor { get; set; } //MstFloor
        public MstFloorplan Floorplan { get; set; }
        public ICollection<PatrolRouteAreas> PatrolRouteAreas { get; set; } = new List<PatrolRouteAreas>();
    }
    //PatrolRoute
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
        public ICollection<PatrolAssignment> PatrolAssignments { get; set; } = new List<PatrolAssignment>();

    }
    //PatrolRouteAreas
    [Index(nameof(PatrolRouteId))]
    [Index(nameof(PatrolAreaId))]
    public class PatrolRouteAreas : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("patrol_route_id")]
        public Guid PatrolRouteId { get; set; }
        [Column("patrol_area_id")]
        public Guid PatrolAreaId { get; set; }
        [Column("order_index")]
        public int OrderIndex { get; set; }
        [Column("estimated_distance")]
        public float EstimatedDistance { get; set; }
        [Column("estimated_time")]
        public int EstimatedTime { get; set; }
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
    //PatrolRouteTimeGroups
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
    //PatrolAssignment
    public class PatrolAssignment : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("patrol_route_id")]
        public Guid? PatrolRouteId { get; set; }
        [Column("start_date")]
        public DateTime? StartDate { get; set; }
        [Column("end_date")]
        public DateTime? EndDate { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<PatrolAssignmentSecurity> PatrolAssignmentSecurities { get; set; } = new List<PatrolAssignmentSecurity>();
    }
    //PatrolAssignmentSecurity
    public class PatrolAssignmentSecurity : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_assignment_id")]
        public Guid PatrolAssignmentId { get; set; }
        [Column("security_id")]
        public Guid SecurityId { get; set; }
        public PatrolAssignment? PatrolAssignment { get; set; }
        public MstSecurity? Security { get; set; }
        public MstApplication? Application { get; set; }
    }
}