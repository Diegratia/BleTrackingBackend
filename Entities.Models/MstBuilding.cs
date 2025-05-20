using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstBuilding : BaseModel
    {

        [Required]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("image")]
        public string Image { get; set; } 

        [Required]
        [Column("application_id")]
        [ForeignKey("Application")]
        public Guid ApplicationId { get; set; }

        [StringLength(255)]
        [Column("created_by")]
        public string CreatedBy { get; set; } = "";

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [StringLength(255)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; } = "";

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstApplication Application { get; set; }
        public virtual ICollection<MstBuilding> Buildings { get; set; } = new List<MstBuilding>();
    }
}