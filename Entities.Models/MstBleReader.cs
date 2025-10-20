using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstBleReader : BaseModelWithTime, IApplicationEntity
    {

        [Required]
        [ForeignKey("Brand")]
        [Column("brand_id")]
        public Guid BrandId { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Column("ip")]
        public string? Ip { get; set; }

        [StringLength(255)]
        [Column("gmac")]
        public string? Gmac { get; set; }

        // [Required]
        // [StringLength(255)]
        // [Column("engine_reader_id")]
        // public string EngineReaderId { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        [Required]
        [ForeignKey(nameof(Application))]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public MstApplication Application { get; set; }
        public MstBrand Brand { get; set; }
        public ICollection<TrackingTransaction> TrackingTransactions { get; set; } = new List<TrackingTransaction>();
        public ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}