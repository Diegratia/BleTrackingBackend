using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstBuilding : BaseModelWithTimeInt, IApplicationEntity
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

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstApplication Application { get; set; }
        public virtual ICollection<MstBuilding> Buildings { get; set; } = new List<MstBuilding>();
    }
}