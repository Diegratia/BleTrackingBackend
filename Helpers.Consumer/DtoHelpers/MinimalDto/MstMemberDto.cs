using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;

namespace Data.ViewModels.Dto.Helpers.MinimalDto
{
    public class MstMemberDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public long Generate { get; set; }
        // public string? PersonId { get; set; }
        // public Guid? OrganizationId { get; set; }
        // public Guid? DepartmentId { get; set; }
        // public Guid? DistrictId { get; set; }
        // public string? IdentityId { get; set; }
        // public string? CardNumber { get; set; }
        // public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        // public string? Phone { get; set; }
        // public string? Email { get; set; }
        // public string? Gender { get; set; } // Enum sebagai string
        // public string? Address { get; set; }
        // public string? FaceImage { get; set; }
        // public int? UploadFr { get; set; }
        // public string? UploadFrError { get; set; }
        // public DateOnly? BirthDate { get; set; }
        // public DateOnly? JoinDate { get; set; }
        // public DateOnly? ExitDate { get; set; }
        // public string? HeadMember1 { get; set; }
        // public string? HeadMember2 { get; set; }
        public Guid ApplicationId { get; set; }
        // public string? StatusEmployee { get; set; } // Enum sebagai string
        // public string CreatedBy { get; set; }
        // public DateTime CreatedAt { get; set; }
        // public string UpdatedBy { get; set; }
        // public DateTime UpdatedAt { get; set; }
        // public int? Status { get; set; }
        // public MstOrganizationDto? Organization { get; set;}
        // public MstDepartmentDto? Department { get; set; }
        // public MstDistrictDto? District { get; set; }
    }
}