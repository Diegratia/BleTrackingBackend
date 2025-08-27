using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data.ViewModels.Dto.Helpers.MinimalDto
{
    public class VisitorDto : BaseModelDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        // public string? PersonId { get; set; }
        // public string? IdentityId { get; set; }
        // public string? IdentityType { get; set; }
        // public string? VisitorActiveStatus { get; set; }
        // public string? CardNumber { get; set; }
        // public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        // public string? Phone { get; set; }
        // public string? Email { get; set; }
        // public string? Gender { get; set; }
        // public string? Address { get; set; }
        // public long? VisitorGroupCode { get; set; }
        // public string? VisitorNumber { get; set; }
        // public string? VisitorCode { get; set; }
        // public string? OrganizationName { get; set; }
        // public string? DistrictName { get; set; }
        // public string? DepartmentName { get; set; }
        // public bool? IsVip { get; set; }
        // public string? FaceImage { get; set; }
        // public int? UploadFr { get; set; } = 0;
        // public string? UploadFrError { get; set; }
        // public DateTime? CreatedAt { get; set; }
        // public string? CreatedBy { get; set; }
        // public DateTime? UpdatedAt { get; set; }
        // public string? UpdatedBy { get; set; }
        public Guid ApplicationId { get; set; }
        // public string Status { get; set; }
    }

}