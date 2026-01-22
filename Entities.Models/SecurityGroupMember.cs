using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class SecurityGroupMember : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("security_group_id")]
        public Guid? SecurityGroupId { get; set; }
        [Column("security_id")]
        public Guid? SecurityId { get; set; }
        public MstApplication? Application { get; set; }
        public SecurityGroup? SecurityGroup { get; set; }
        public MstSecurity? Security { get; set; }
        
    }
}