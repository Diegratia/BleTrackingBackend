using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data.ViewModels
{
    public class VisitorDto : BaseModelDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        // public string? VisitorActiveStatus { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsVip { get; set; }
        public string? FaceImage { get; set; }
        public Guid? CardId { get; set; }
        public int? UploadFr { get; set; } = 0;
        public string? UploadFrError { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }

        public CardDto Card { get; set; }
    }
    
     public class OpenVisitorDto : BaseModelDto
    {
        public long Generate { get; set; }

        [JsonPropertyName("visitor_id")]
        public Guid Id { get; set; }
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        // public string? VisitorActiveStatus { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsVip { get; set; }
        public string? FaceImage { get; set; }
        public Guid? CardId { get; set; }
        public int? UploadFr { get; set; } = 0;
        public string? UploadFrError { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid ApplicationId { get; set; }
        public string Status { get; set; }

        // public CardDto Card { get; set; }
    }

    public class VisitorCreateDto : TrxVisitorCreateDto
    {
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public long? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; }
        public string? VisitorCode { get; set; }
        public bool? IsVip { get; set; }
        public IFormFile? FaceImage { get; set; }

        public Guid ApplicationId { get; set; }
    }
    


     public class OpenVisitorCreateDto : TrxVisitorCreateDto
    {
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        public string? CardNumber { get; set; }
        public Guid? cardId { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public long? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; }
        public string? VisitorCode { get; set; }
        public bool? IsVip { get; set; }
        public IFormFile? FaceImage { get; set; }
        public Guid CardId { get; set; }
        public Guid ApplicationId { get; set; }

    }
    

    public class VisitorUpdateDto
    {
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? VisitorStatus { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public long? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; }
        public string? VisitorCode { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsVip { get; set; }
        public string? VisitorActiveStatus { get; set; }

        // public bool? IsInvitationAccepted { get; set; }
        // public DateTime? EmailVerficationSendAt { get; set; }
        // public string? EmailVerificationToken { get; set; }
        // public DateTime? VisitorPeriodStart { get; set; }
        // public DateTime? VisitorPeriodEnd { get; set; }   
        public IFormFile? FaceImage { get; set; }
        // public Guid ApplicationId { get; set; }
    }

    public class ConfirmVisitorEmailDto
    {
        public string Email { get; set; }
        public string ConfirmationCode { get; set; }
    }


    public class VisitorWithTrxCreateDto
    {
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public long? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; }
        public string? VisitorCode { get; set; }
        public bool? IsVip { get; set; }
        public IFormFile? FaceImage { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public DateTime? VisitorPeriodStart { get; set; }
        public DateTime? VisitorPeriodEnd { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public Guid? ParkingId { get; set; }
        public Guid? VisitorId { get; set; }
    }

    public class CreateInvitationDto
    {
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public DateTime? VisitorPeriodStart { get; set; }
        public DateTime? VisitorPeriodEnd { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public Guid? ParkingId { get; set; }
    }

    public class SendEmailInvitationDto : TrxVisitorInvitationDto
    {
        public string Email { get; set; }
        public string? Name { get; set; }
        public bool? IsVip { get; set; }
        public Guid? MaskedAreaId { get; set; }
    }


    public class VisitorInvitationDto : TrxVisitorInvitationDtoNoDate
    {
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        // public string? VisitorActiveStatus { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
        public bool? IsVip { get; set; }
        // public bool? IsInvitationAccepted { get; set; }     
        public IFormFile? FaceImage { get; set; }
        [FromQuery]
        public Guid ApplicationId { get; set; }
        [FromQuery]
        public string? InvitationCode { get; set; } // <- ini penting untuk akses
    }

    public class MemberInvitationDto
    {
        [FromQuery]
        public Guid ApplicationId { get; set; }
        [FromQuery]
        public string? InvitationCode { get; set; }

    }
}