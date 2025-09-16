using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
    {
        public class MstDistrict : BaseModelWithTimeInt, IApplicationEntity
        {

            [Required]
            [StringLength(255)]
            [Column("code")]
            public string Code { get; set; }

            [Required]
            [StringLength(255)]
            [Column("name")]
            public string Name { get; set; }

            [Required]
            [StringLength(255)]
            [Column("district_host")]
            public string DistrictHost { get; set; }

            [Required]
            [Column("application_id")]
            public Guid ApplicationId { get; set; }

            [Required]
            [Column("status")]
            public int Status { get; set; } = 1;

        
            public MstApplication Application { get; set; }

            public ICollection<MstMember> Members { get; set; } = new List<MstMember>();
        }
    }