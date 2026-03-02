using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstDepartment : BaseModelWithTimeInt, IApplicationEntity
    {

        [StringLength(255)]
        [Column("code")]
        public string? Code { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Column("department_host")]
        public string? DepartmentHost { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; } 

        [Required]
        [Column("status")]
        public int Status { get; set; } = 1;

        public MstApplication Application { get; set; }
        // public ICollection<MstBuilding> Buildings { get; set; } = new List<MstBuilding>();
        public ICollection<MstMember> Members { get; set; } = new List<MstMember>();
    }
}