using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstMember : BaseModel
    {
        [Required]
        [StringLength(255)]
        [Column("person_id")]
        public string PersonId { get; set; }

        [Required]
        [ForeignKey("Organization")]
        [Column("organization_id")]
        public Guid OrganizationId { get; set; }

        [Required]
        [ForeignKey("Department")]
        [Column("department_id")]
        public Guid DepartmentId { get; set; }

        [Required]
        [ForeignKey("District")]
        [Column("district_id")]
        public Guid DistrictId { get; set; }

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
        [Column("birth_date")]
        public DateOnly BirthDate { get; set; }

        [Required]
        [Column("join_date")]
        public DateOnly JoinDate { get; set; }

        [Required]
        [Column("exit_date")]
        public DateOnly ExitDate { get; set; } = DateOnly.MaxValue;

        [Required]
        [StringLength(255)]
        [Column("head_member1")]
        public string HeadMember1 { get; set; }

        [Required]
        [StringLength(255)]
        [Column("head_member2")]
        public string HeadMember2 { get; set; }

        [Required]
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("status_employee")]
        public StatusEmployee StatusEmployee { get; set; }

        [Required]
        [StringLength(255)]
        [Column("created_by")]
        public string CreatedBy { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [StringLength(255)]
        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; } = 1;

        public virtual MstApplication Application { get; set; }

        public virtual MstOrganization Organization { get; set; }

        public virtual MstDepartment Department { get; set; }

        public virtual MstDistrict District { get; set; }

        public virtual ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
    }
}