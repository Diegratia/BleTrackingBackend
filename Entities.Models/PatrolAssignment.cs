using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class PatrolAssignment : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("patrol_route_id")]
        public Guid? PatrolRouteId { get; set; }
        [Column("security_group_id")]
        public Guid? SecurityGroupId { get; set; }
        [Column("time_group_id")]
        public Guid? TimeGroupId { get; set; }
        [Column("start_date")]
        public DateTime? StartDate { get; set; }
        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        public PatrolRoute? PatrolRoute { get; set; }
        public SecurityGroup? SecurityGroup { get; set; }
        public TimeGroup? TimeGroup { get; set; }
        public MstApplication? Application { get; set; }
        
    }
}