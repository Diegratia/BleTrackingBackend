using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Data.ViewModels
{    public class VisitorDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public string PersonId { get; set; }
        public string IdentityId { get; set; }
        public string CardNumber { get; set; }
        public string BleCardNumber { get; set; }
        public string VisitorType { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid DepartmentId { get; set; }
        public bool IsVip { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime EmailVerficationSendAt { get; set; }
        public string EmailVerificationToken { get; set; }
        public DateTime VisitorPeriodStart { get; set; }
        public DateTime VisitorPeriodEnd { get; set; }
        public bool IsEmployee { get; set; }       
        public string FaceImage { get; set; }
        public int UploadFr { get; set; } = 0;
        public string UploadFrError { get; set; }
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }

        public MstOrganizationDto Organization { get; set; }
        public MstDistrictDto District { get; set; }
        public MstDepartmentDto Department { get; set; }
    }

    public class VisitorCreateDto
    {
        public string PersonId { get; set; }
        public string IdentityId { get; set; }
        public string CardNumber { get; set; }
        public string BleCardNumber { get; set; }
        public string VisitorType { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid DepartmentId { get; set; }
        public bool IsVip { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime EmailVerficationSendAt { get; set; }
        public string EmailVerificationToken { get; set; }
        public DateTime VisitorPeriodStart { get; set; }
        public DateTime VisitorPeriodEnd { get; set; }
        public bool IsEmployee { get; set; }        
        public IFormFile FaceImage { get; set; }
        public Guid ApplicationId { get; set; }

    }

    public class VisitorUpdateDto
    {
        public string PersonId { get; set; }
        public string IdentityId { get; set; }
        public string CardNumber { get; set; }
        public string BleCardNumber { get; set; }
        public string VisitorType { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid DistrictId { get; set; }
        public Guid DepartmentId { get; set; }
        public bool IsVip { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime EmailVerficationSendAt { get; set; }
        public string EmailVerificationToken { get; set; }
        public DateTime VisitorPeriodStart { get; set; }
        public DateTime VisitorPeriodEnd { get; set; }
        public bool IsEmployee { get; set; }        
        public IFormFile FaceImage { get; set; }
        public Guid ApplicationId { get; set; }
    }
}