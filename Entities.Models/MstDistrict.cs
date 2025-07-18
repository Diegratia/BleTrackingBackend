using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
    {
        public class MstDistrict
        {
            [Required]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column("_generate")]
            public int Generate { get; set; } 

            [Key]
            [Column("id")]
            public Guid Id { get; set; } = Guid.NewGuid(); 

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
            [ForeignKey("Application")]
            [Column("application_id")]
            public Guid ApplicationId { get; set; }

            [Required]
            [StringLength(255)]
            [Column("created_by")]
            public string CreatedBy { get; set; }

            [Required]
            [Column("created_at")]
            public DateTime CreatedAt { get; set; }

            [Required]
            [StringLength(255)]
            [Column("updated_by")]
            public string UpdatedBy { get; set; }

            [Required]
            [Column("updated_at")]
            public DateTime UpdatedAt { get; set; }

            [Required]
            [Column("status")]
            public int? Status { get; set; } = 1;

        
            public virtual MstApplication Application { get; set; }

            public virtual ICollection<MstMember> Members { get; set; } = new List<MstMember>();
        }
    }