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
        public int Generate { get; set; }

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Mac { get; set; }

        [Required]
        [StringLength(255)]
        public string Ip { get; set; }

        [Required]
        public decimal LocationX { get; set; }

        [Required]
        public decimal LocationY { get; set; }

        [Required]
        public long LocationPxX { get; set; }

        [Required]
        public long LocationPxY { get; set; }

        [Required]
        [StringLength(255)]
        public string EngineReaderId { get; set; }

        [Required]
        [StringLength(255)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required]
        public int? Status { get; set; } = 1;

        public virtual MstBrand Brand { get; set; }
        public virtual AlarmRecordTracking AlarmRecordTracking { get; set; }

        public virtual ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}