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
            public int Generate { get; set; } 

            [Key]
            public Guid Id { get; set; } = Guid.NewGuid(); 

            [Required]
            [StringLength(255)]
            public string Code { get; set; }

            [Required]
            [StringLength(255)]
            public string Name { get; set; }

            [Required]
            [StringLength(255)]
            public string DistrictHost { get; set; }

            [Required]
            [ForeignKey("Application")]
            public Guid ApplicationId { get; set; }

            [Required]
            [StringLength(255)]
            public string CreatedBy { get; set; }

            [Required]
            public DateTime CreatedAt { get; set; }

            [Required]
            [StringLength(255)]
            public string UpdatedBy { get; set; }

            [Required]
            public DateTime UpdatedAt { get; set; }

            [Required]
            public int? Status { get; set; } = 1;

        
            public virtual MstApplication Application { get; set; }

            public virtual ICollection<MstMember> Members { get; set; } = new List<MstMember>();
        }
    }