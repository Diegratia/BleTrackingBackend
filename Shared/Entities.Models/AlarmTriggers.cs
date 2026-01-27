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
                [Column("beacon_id", TypeName = "nvarchar(16)")]
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

                [Column("visitor_id")]
                public Guid? VisitorId { get; set; }
                [Column("member_id")]
                public Guid? MemberId { get; set; }

                [Column("security_id")]
                public Guid? SecurityId { get; set; }

                [Column("trigger_time")]
                public DateTime? TriggerTime { get; set; } // first trigger times
                [Column("alarm_color")]
                public string? AlarmColor { get; set; }

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

                [Column("action_updated_at")]
                public DateTime? ActionUpdatedAt { get; set; }

                [Column("last_seen_at")]
                public DateTime? LastSeenAt { get; set; }

                [Column("last_notified_at")]
                public DateTime? LastNotifiedAt { get; set; }

                [Required]
                [ForeignKey("Application")]
                [Column("application_id")]
                public Guid ApplicationId { get; set; }
                public MstApplication Application { get; set; }
                public MstFloorplan Floorplan { get; set; }
                public Visitor Visitor { get; set; }
                public MstMember Member { get; set; } 
                public MstSecurity Security { get; set; } 
            }
        }