using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Shared.Contracts;


namespace Entities.Models
{
    public class AlarmRecordTracking : BaseModel, IApplicationEntity
    {
        
        [Column("timestamp")]
        public DateTime? Timestamp { get; set; }

        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }

        [Column("member_id")]
        public Guid? MemberId { get; set; }

        [Column("ble_reader_id")]
        public Guid? ReaderId { get; set; }

        [Column("alarm_triggers_id")]
        public Guid? AlarmTriggersId { get; set; }

        [Column("floorplan_masked_area_id")]
        public Guid? FloorplanMaskedAreaId { get; set; }

        [Column("alarm_record_status")]
        public AlarmRecordStatus? Alarm { get; set; }

        [Column("action")]
        public ActionStatus? Action { get; set; }

        [Required]
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

        public MstApplication Application { get; set; }
        public Visitor Visitor { get; set; }
        public MstMember Member { get; set; }
        public MstBleReader Reader { get; set; }
        public FloorplanMaskedArea FloorplanMaskedArea{ get; set; } 
        public AlarmTriggers AlarmTriggers{ get; set; } 
    }
}