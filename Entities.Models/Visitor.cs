using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class Visitor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Generate { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string PersonId { get; set; }

        [Required]
        [StringLength(255)]
        public string IdentityId { get; set; }

        [Required]
        [StringLength(255)]
        public string CardNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string BleCardNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Phone { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string FaceImage { get; set; }

        [Required]
        public int UploadFr { get; set; } = 0;

        [Required]
        public string UploadFrError { get; set; }

        [Required]
        [ForeignKey("Application")]
        public Guid ApplicationId { get; set; }

        [Required]
        public DateTime RegisteredDate { get; set; }

        [Required]
        public DateTime VisitorArrival { get; set; }

        [Required]
        public DateTime VisitorEnd { get; set; }

        [Required]
        public long PortalKey { get; set; }

        [Required]
        public DateTime TimestampPreRegistration { get; set; }

        [Required]
        public DateTime TimestampCheckedIn { get; set; }

        [Required]
        public DateTime TimestampCheckedOut { get; set; }

        [Required]
        public DateTime TimestampDeny { get; set; }

        [Required]
        public DateTime TimestampBlocked { get; set; }

        [Required]
        public DateTime TimestampUnblocked { get; set; }

        [Required]
        [StringLength(255)]
        public string CheckinBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string CheckoutBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string DenyBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string BlockBy { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string UnblockBy { get; set; } ="";

        [Required]
        [StringLength(255)]
        public string ReasonDeny { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string ReasonBlock { get; set; } = "";

        [Required]
        [StringLength(255)]
        public string ReasonUnblock { get; set; } = "";

        [Required]
        public VisitorStatus Status { get; set; }

        public virtual MstApplication Application { get; set; }

        public virtual AlarmRecordTracking AlarmRecordTracking { get; set; }
        public virtual ICollection<VisitorBlacklistArea> BlacklistAreas { get; set; } = new List<VisitorBlacklistArea>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        
    }
}