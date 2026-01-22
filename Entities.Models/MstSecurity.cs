using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstSecurity : BaseModelWithTime, IApplicationEntity
    {
        [Column("person_id")]
        public string? PersonId { get; set; }

        [Column("organization_id")]
        public Guid? OrganizationId { get; set; }

        [Column("department_id")]
        public Guid? DepartmentId { get; set; }

        [Column("district_id")]
        public Guid? DistrictId { get; set; }

        [Column("identity_id")]
        public string? IdentityId { get; set; }

        [Column("card_number")]
        public string? CardNumber { get; set; }

        [Column("ble_card_number")]
        public string? BleCardNumber { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("email")]
        public string? Email { get; set; }
        [Column("gender")]
        public Gender? Gender { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("face_image")]
        public string? FaceImage { get; set; }

        [Column("upload_fr")]
        public int UploadFr { get; set; } = 0;

        [Column("upload_fr_error")]
        public string? UploadFrError { get; set; }

        [Column("birth_date")]
        public DateOnly? BirthDate { get; set; }

        [Column("join_date")]
        public DateOnly? JoinDate { get; set; }

        [Column("exit_date")]
        public DateOnly? ExitDate { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("status_employee")]
        public StatusEmployee? StatusEmployee { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        public MstApplication Application { get; set; }
        public MstOrganization Organization { get; set; }
        public MstDepartment Department { get; set; }
        public MstDistrict District { get; set; }
        public ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
        public ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public ICollection<AlarmTriggers> AlarmTriggers { get; set; } = new List<AlarmTriggers>();
        public ICollection<PatrolAssignment> PatrolAssignments { get; set; } = new List<PatrolAssignment>();
    }
}