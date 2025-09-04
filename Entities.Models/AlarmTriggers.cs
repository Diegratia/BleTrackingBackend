using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class AlarmTriggers : BaseModelOnlyId, IApplicationEntity
    {
        [Column("beacon_id")]
        public string? BeaconId { get; set; }

        [Column("floorplan_id")]
        public Guid? FloorplanId { get; set; }

        [Column("pos_x")]
        public float? PosX { get; set; }

        [Column("pos_y")]
        public float? PosY { get; set; }

        [Column("is_in_restricted_area")]
        public bool? IsInRestrictedArea { get; set; }

        [Column("first_gateway_id")]
        public string FirstGatewayId { get; set; }

        [Column("second_gateway_id")]
        public string SecondGatewayId { get; set; }

        [Column("first_distance")]
        public float? FirstDistance { get; set; }

        [Column("second_distance")]
        public float? SecondDistance { get; set; }

        [Column("trigger_time")]
        public DateTime? TriggerTime { get; set; }

        [Column("alarm_record_status")]
        public AlarmRecordStatus? Alarm { get; set; }

        [Column("action")]
        public ActionStatus? Action { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

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

        [Column("idle_by")]
        public string? IdleBy { get; set; }

        [Column("done_by")]
        public string? DoneBy { get; set; }

        [Column("cancel_by")]
        public string? CancelBy { get; set; }

        [Column("waiting_by")]
        public string? WaitingBy { get; set; }

        [Column("investigated_by")]
        public string? InvestigatedBy { get; set; }

        [Column("investigated_result")]
        public string? InvestigatedResult { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application { get; set; }

        public virtual MstFloorplan Floorplan { get; set; }
    }
}