using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class BleReaderNode
    {
        [Column("_generate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Generate { get; set; }

        [Key]
        [Column("id")]
        public Guid? Id { get; set; } = Guid.NewGuid(); 

        
        [ForeignKey("Reader")]
        [Column("ble_reader_id")]
        public Guid? ReaderId { get; set; }

        [Required]
        [Column("start_pos")]
        public int StartPos { get; set; }

        [Required]
        [Column("end_pos")]
        public int EndPos { get; set; }

        [Required]
        [Column("distance_px")]
        public decimal DistancePx { get; set; }

        [Required]
        [Column("distance")]
        public decimal Distance { get; set; }

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

         public virtual MstApplication Application { get; set; }
         public virtual MstBleReader Reader { get; set; }
        
    }
}