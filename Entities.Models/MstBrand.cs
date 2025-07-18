using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
   public class MstBrand
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
        [Column("name")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("tag")]
        public string Tag { get; set; }
        
        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        // public virtual ICollection<MstIntegration> Integrations { get; set; } = new List<MstIntegration>(); 
        // public virtual ICollection<MstBleReader> BleReaders { get; set; } = new List<MstBleReader>();
        
    }
}
