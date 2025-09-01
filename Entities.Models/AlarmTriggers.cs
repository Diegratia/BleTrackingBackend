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

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual MstFloorplan Floorplan { get; set; }
 
    }
}