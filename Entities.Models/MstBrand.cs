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
        public int Generate { get; set; } 

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); 
        
        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Tag { get; set; }
        
        [Required]
        public int? Status { get; set; } = 1;

        // public virtual ICollection<MstIntegration> Integrations { get; set; } = new List<MstIntegration>(); 
        // public virtual ICollection<MstBleReader> BleReaders { get; set; } = new List<MstBleReader>();
        
    }
}

