using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class Visitor : BaseModel
    {
        [Required]
        [StringLength(255)]
        [Column("person_id")]
        public string PersonId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("identity_id")]
        public string IdentityId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("card_number")]
        public string CardNumber { get; set; }

        [Required]
        [StringLength(255)]
        [Column("ble_card_number")]
        public string BleCardNumber { get; set; }

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        [Column("phone")]
        public string Phone { get; set; }

        [Required]
        [StringLength(255)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("gender")]
        public Gender Gender { get; set; }

        [Required]
        [Column("address")]
        public string Address { get; set; }

        [Required]
        [Column("face_image")]
        public string FaceImage { get; set; }

        [Required]
        [Column("upload_fr")]
        public int UploadFr { get; set; } = 0;

        [Required]
        [Column("upload_fr_error")]
        public string UploadFrError { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("registered_date")]
        public DateTime RegisteredDate { get; set; }

        [Required]
        [Column("visitor_arrival")]
        public DateTime VisitorArrival { get; set; }

        [Required]
        [Column("visitor_end")]
        public DateTime VisitorEnd { get; set; }

        [Required]
        [Column("portal_key")]
        public long PortalKey { get; set; }

        [Required]
        [Column("timestamp_pre_registration")]
        public DateTime TimestampPreRegistration { get; set; }

        [Required]
        [Column("timestamp_checked_in")]
        public DateTime TimestampCheckedIn { get; set; }

        [Required]
        [Column("timestamp_checked_out")]
        public DateTime TimestampCheckedOut { get; set; }

        [Required]
        [Column("timestamp_deny")]
        public DateTime TimestampDeny { get; set; }

        [Required]
        [Column("timestamp_blocked")]
        public DateTime TimestampBlocked { get; set; }

        [Required]
        [Column("timestamp_unblocked")]
        public DateTime TimestampUnblocked { get; set; }

        [Required]
        [StringLength(255)]
        [Column("checkin_by")]
        public string CheckinBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        [Column("checkout_by")]
        public string CheckoutBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        [Column("deny_by")]
        public string DenyBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        [Column("block_by")]
        public string BlockBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        [Column("unblock_by")]
        public string UnblockBy { get; set; } ="";

        [Required]
        [StringLength(255)]
        [Column("reason_deny")]
        public string ReasonDeny { get; set; } = "";

        [Required]
        [StringLength(255)]
        [Column("reason_block")]
        public string ReasonBlock { get; set; } = "";

        [Required]
        [StringLength(255)]
        [Column("reason_unblock")]
        public string ReasonUnblock { get; set; } = "";

        [Required]
        [Column("status")]
        public VisitorStatus Status { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual AlarmRecordTracking AlarmRecordTracking { get; set; }
        public virtual ICollection<VisitorBlacklistArea> BlacklistAreas { get; set; } = new List<VisitorBlacklistArea>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
    }
}