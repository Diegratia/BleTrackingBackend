using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class SecurityGroup : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<SecurityGroupMember> SecurityGroupMembers { get; set; } = new List<SecurityGroupMember>();
        public ICollection<PatrolAssignment> PatrolAssignments { get; set; } = new List<PatrolAssignment>();
        
    }
}