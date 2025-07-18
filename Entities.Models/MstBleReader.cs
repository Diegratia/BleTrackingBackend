using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstBleReader 
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("_generate")]
        public int Generate { get; set; }

        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Brand")]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        [Column("ip")]
        public string Ip { get; set; }

        [Required]
        [StringLength(255)]
        [Column("gmac")]
        public string Gmac { get; set; }

        [Required]
        [StringLength(255)]
        [Column("engine_reader_id")]
        public string EngineReaderId { get; set; }

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

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstBrand Brand { get; set; }
        public virtual AlarmRecordTracking AlarmRecordTracking { get; set; }

        public virtual ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}