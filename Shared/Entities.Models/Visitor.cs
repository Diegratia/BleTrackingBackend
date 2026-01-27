using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Shared.Contracts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Entities.Models
{
    public class Visitor : BaseModelWithTime, IApplicationEntity
    {
        [Column("person_id")]
        public string? PersonId { get; set; } // nrp, no pegawai, kalau ga ada isi ktp

        [StringLength(255)]
        [Column("identity_id")]
        public string? IdentityId { get; set; } //ktp sim nik

        [Column("identity_type")]
        public IdentityType? IdentityType { get; set; }

        [StringLength(255)]
        [Column("card_number")]
        public string? CardNumber { get; set; }

        [StringLength(255)]
        [Column("ble_card_number")]
        public string? BleCardNumber { get; set; }

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

        [Column("organization_name")]
        public string? OrganizationName { get; set; }

        [Column("district_name")]
        public string? DistrictName { get; set; }

        [Column("department_name")]
        public string? DepartmentName { get; set; }

        [Column("visitor_group_code")]
        public long? VisitorGroupCode { get; set; }

        [Column("visitor_number")]
        public string? VisitorNumber { get; set; }

        [Column("visitor_code")]
        public string? VisitorCode { get; set; }

        [Column("is_vip")]
        public bool? IsVip { get; set; }

        [Column("is_blacklist")]
        public bool? IsBlacklist { get; set; }

        [Column("blacklist_reason")]
        public string? BlacklistReason { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("face_image")]
        public string? FaceImage { get; set; }

        [Column("upload_fr")]
        public int UploadFr { get; set; } = 0;

        [Column("upload_fr_error")]
        public string? UploadFrError { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }
        // public MstOrganization Organization { get; set; }
        // public MstDepartment Department { get; set; }
        // public MstDistrict District { get; set; }
        public MstApplication Application { get; set; }
        public ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
        public ICollection<TrxVisitor> TrxVisitors { get; set; } = new List<TrxVisitor>();
    }
}