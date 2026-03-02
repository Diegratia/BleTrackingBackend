using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstOrganization : BaseModelWithTimeInt, IApplicationEntity
    {

        [StringLength(255)]
        [Column("code")]
        public string? Code { get; set; }
    
        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Column("organization_host")]
        public string? OrganizationHost { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("status")]
        public int Status { get; set; }

        public MstApplication Application { get; set; }

        public ICollection<MstMember> Members { get; set; } = new List<MstMember>();
    }
}