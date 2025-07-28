using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstBrand : BaseModelIntGenerate
    {

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        [Column("tag")]
        public string Tag { get; set; }


        [Required]
        [ForeignKey(nameof(Application))]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstApplication Application { get; set; }

        // public virtual ICollection<MstIntegration> Integrations { get; set; } = new List<MstIntegration>(); 
        // public virtual ICollection<MstBleReader> BleReaders { get; set; } = new List<MstBleReader>();

    }
}
