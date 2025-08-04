using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class AlarmRecordTracking : BaseModel, IApplicationEntity
    {
        
        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        [Required]
        [ForeignKey("Visitor")]
        [Column("visitor_id")]
        public Guid VisitorId { get; set; }

        [Required]
        [ForeignKey("Reader")]
        [Column("ble_reader_id")]
        public Guid ReaderId { get; set; }

        [Required]
        [ForeignKey("FloorplanMaskedArea")]
        [Column("floorplan_masked_area_id")]
        public Guid FloorplanMaskedAreaId { get; set; }

        [Column("alarm_record_status")]
        public AlarmRecordStatus? Alarm { get; set; }

        [Column("action")]
        public ActionStatus? Action { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("idle_timestamp")]
        public DateTime? IdleTimestamp { get; set; }

        [Column("done_timestamp")]
        public DateTime? DoneTimestamp { get; set; }

        [Column("cancel_timestamp")]
        public DateTime? CancelTimestamp { get; set; }

        [Column("waiting_timestamp")]
        public DateTime? WaitingTimestamp { get; set; }

        [Column("investigated_timestamp")]
        public DateTime? InvestigatedTimestamp { get; set; }

        [Column("investigated_done_at")]
        public DateTime? InvestigatedDoneAt { get; set; }

        [StringLength(255)]
        [Column("idle_by")]
        public string? IdleBy { get; set; } 

        [StringLength(255)]
        [Column("done_by")]
        public string? DoneBy { get; set; } 

        [StringLength(255)]
        [Column("cancel_by")]
        public string? CancelBy { get; set; } 

        [StringLength(255)]
        [Column("waiting_by")]
        public string? WaitingBy { get; set; } 

        [StringLength(255)]
        [Column("investigated_by")]
        public string? InvestigatedBy { get; set; }

        [Column("investigated_result")]
        public string? InvestigatedResult { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual Visitor Visitor { get; set; }
        public virtual MstBleReader Reader { get; set; }
        public virtual FloorplanMaskedArea FloorplanMaskedArea{ get; set; } 
    }
}