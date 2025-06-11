using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class RecordTrackingLog
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("table_name")]
        public string TableName { get; set; }

        [Required]
        [ForeignKey("FloorplanDevice")]
        [Column("floorplan_id")]
        public Guid FloorplanId { get; set; }

        [Required]
        [Column("floorplan_timestamp")]
        public DateTime FloorplanTimestamp { get; set; }

        public virtual FloorplanDevice FloorplanDevices { get; set; }
    }
}