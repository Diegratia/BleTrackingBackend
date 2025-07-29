using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Entities.Models
{
    public class Visitor : BaseModel, IApplicationEntity
    {
        [Column("person_id")]
        public string? PersonId { get; set; }

        [StringLength(255)]
        [Column("identity_id")]
        public string? IdentityId { get; set; }

        [StringLength(255)]
        [Column("card_number")]
        public string? CardNumber { get; set; }

        [StringLength(255)]
        [Column("ble_card_number")]
        public string? BleCardNumber { get; set; }

        [StringLength(255)]
        [Column("visitor_type")]
        public VisitorType? VisitorType { get; set; }

        [StringLength(255)]
        [Column("name")]
        public string? Name { get; set; }

        [StringLength(255)]
        [Column("phone")]
        public string? Phone { get; set; }

        [StringLength(255)]
        [Column("email")]
        public string? Email { get; set; }

        [Column("gender")]
        public Gender? Gender { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [ForeignKey(nameof(Organization))]
        [Column("organization_id")]
        public Guid? OrganizationId { get; set; }


        [ForeignKey(nameof(District))]
        [Column("district_id")]
        public Guid? DistrictId { get; set; }

        [ForeignKey(nameof(Department))]
        [Column("department_id")]
        public Guid? DepartmentId { get; set; }

        [Column("is_vip")]
        public bool? IsVip { get; set; }

        [Column("is_email_vervied")]
        public bool? IsEmailVerified { get; set; }

        [Column("email_verification_send_at")]
        public DateTime? EmailVerficationSendAt { get; set; }

        [Column("email_verification_token")]
        public string? EmailVerificationToken { get; set; }

        [Column("visitor_period_start")]
        public DateTime? VisitorPeriodStart { get; set; }

        [Column("visitor_period_end")]
        public DateTime? VisitorPeriodEnd { get; set; }

        [Column("is_employee")]
        public bool? IsEmployee { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("face_image")]
        public string? FaceImage { get; set; }

        [Column("upload_fr")]
        public int UploadFr { get; set; } = 0;

        [Column("upload_fr_error")]
        public string? UploadFrError { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }
        public virtual MstOrganization Organization { get; set; }
        public virtual MstDepartment Department { get; set; }
        public virtual MstDistrict District { get; set; }
        public virtual MstApplication Application { get; set; }
        public virtual ICollection<VisitorBlacklistArea> BlacklistAreas { get; set; } = new List<VisitorBlacklistArea>();
        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
    }
}