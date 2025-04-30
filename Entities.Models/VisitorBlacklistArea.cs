using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class VisitorBlacklistArea
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Generate { get; set; } 

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); 

        [Required]
        [ForeignKey("FloorplanMaskedArea")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Required]
        [ForeignKey("Visitor")]
        public Guid VisitorId { get; set; }

        public virtual FloorplanMaskedArea FloorplanMaskedArea { get; set; }
 
        public virtual Visitor Visitor { get; set; }
    }
}