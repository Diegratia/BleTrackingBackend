using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Helpers.Consumer
{
    public class BaseModelWithTimeInt
    {
        [Required]
        [Column("_generate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Generate { get; set; }

        [Required]
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [StringLength(255)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    } 
}