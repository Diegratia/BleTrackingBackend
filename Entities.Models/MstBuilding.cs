using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;


namespace Entities.Models
{
    public class MstBuilding : BaseModelWithTime, IApplicationEntity
    {

        [Column("name")]
        public string? Name { get; set; }

        [Column("image")]
        public string? Image { get; set; }

        [Required]
        [ForeignKey("Application")]
        public Guid ApplicationId { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        public MstApplication Application { get; set; }
        // public ICollection<MstFloor> Floors { get; set; } = new List<MstFloor>();
    }
}