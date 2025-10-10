using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{
    public class MstMemberDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public long Generate { get; set; }
        public string? PersonId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? IdentityId { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; } // Enum sebagai string
        public string? Address { get; set; }
        public string? FaceImage { get; set; }
        public int? UploadFr { get; set; }
        public string? UploadFrError { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateOnly? JoinDate { get; set; }
        public DateOnly? ExitDate { get; set; }
        public string? HeadMember1 { get; set; }
        public string? HeadMember2 { get; set; }
        public Guid ApplicationId { get; set; }
        public string? StatusEmployee { get; set; } // Enum sebagai string
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        public MstOrganizationDto? Organization { get; set;}
        public MstDepartmentDto? Department { get; set; }
        public MstDistrictDto? District { get; set; }
    }

     public class OpenMstMemberDto : BaseModelDto
    {
        public long Generate { get; set; }

        [JsonPropertyName("member_id")]
        public Guid Id { get; set; }
        public string? PersonId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? IdentityId { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; } // Enum sebagai string
        public string? Address { get; set; }
        public string? FaceImage { get; set; }
        public int? UploadFr { get; set; }
        public string? UploadFrError { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateOnly? JoinDate { get; set; }
        public DateOnly? ExitDate { get; set; }
        public string? HeadMember1 { get; set; }
        public string? HeadMember2 { get; set; }
        public Guid ApplicationId { get; set; }
        public string? StatusEmployee { get; set; } // Enum sebagai string
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
        // public MstOrganizationDto? Organization { get; set;}
        // public MstDepartmentDto? Department { get; set; }
        // public MstDistrictDto? District { get; set; }
    }

     public class MstMemberCreateDto : BaseModelDto
    {
        public string? PersonId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? IdentityId { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public Guid? CardId { get; set; }
        public string? Gender { get; set; } // Enum sebagai string
        public string? Address { get; set; }
        public IFormFile? FaceImage { get; set; }
        public int? UploadFr { get; set; }
        public string? UploadFrError { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateOnly? JoinDate { get; set; }
        public string? HeadMember1 { get; set; }
        public string? HeadMember2 { get; set; }
        public Guid ApplicationId { get; set; }
        public string? StatusEmployee { get; set; } // Enum sebagai string
    }

     public class MstMemberUpdateDto 
    {
        public string? PersonId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? IdentityId { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; } // Enum sebagai string
        public string? Address { get; set; }
        public IFormFile? FaceImage { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateOnly? JoinDate { get; set; }
        public Guid? CardId { get; set; }
        public string? HeadMember1 { get; set; }
        public string? HeadMember2 { get; set; }
        public string? StatusEmployee { get; set; } // Enum sebagai string
    }
}