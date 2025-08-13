using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;

namespace Data.ViewModels
{
    public class TrxVisitorDto : BaseModelDto
    {
        public Guid Id { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public DateTime? DenyAt { get; set; }
        public DateTime? BlockAt { get; set; }
        public DateTime? UnblockAt { get; set; }
        public string? CheckinBy { get; set; }
        public string? CheckoutBy { get; set; }
        public string? DenyBy { get; set; }
        public string? DenyReason { get; set; }
        public string? BlockBy { get; set; }
        public string? BlockReason { get; set; }
        public string? Status { get; set; }
        public DateTime? InvitationCreatedAt { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public DateTime? VisitorPeriodStart { get; set; }
        public DateTime? VisitorPeriodEnd { get; set; }
        public bool? IsInvitationAccepted { get; set; }
        public string? InvitationCode { get; set; }
        public int TrxStatus { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public Guid? ParkingId { get; set; }
        public Guid? VisitorId { get; set; }
        public long? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; }
        public string? VisitorCode { get; set; }
        public string? MemberIdentity { get; set; }
        public int? IsMember { get; set; }
        public string? Agenda { get; set; }
        public FloorplanMaskedAreaDto? Maskedarea { get; set; }
        public VisitorDto? Visitor { get; set; }
        public MstMemberDto? Member { get; set; }
    }

    public class TrxVisitorCreateDto : BaseModelDto
    {
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public string? Agenda { get; set; }
        public DateTime? VisitorPeriodStart { get; set; }
        public DateTime? VisitorPeriodEnd { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public Guid? ParkingId { get; set; }
    }

    public class TrxVisitorUpdateDto
    {
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public string? Agenda { get; set; }
        public DateTime? VisitorPeriodStart { get; set; }
        public DateTime? VisitorPeriodEnd { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public Guid? ParkingId { get; set; }
    }

    public class DenyReasonDto
    {
        public string? DenyReason { get; set; }
    }

    public class BlockReasonDto
    {
        public string? BlockReason { get; set; }
    }

    public class TrxVisitorInvitationDto : BaseModelDto
    {
        public string? VehiclePlateNumber { get; set; }
        public string? Agenda { get; set; }
        public Guid? PurposePerson { get; set; }
        public DateTime? VisitorPeriodStart { get; set; }
        public DateTime? VisitorPeriodEnd { get; set; }
    }

    public class TrxVisitorInvitationDtoNoDate : BaseModelDto
    {
        public string? VehiclePlateNumber { get; set; }
        public string? Agenda { get; set; }
        public Guid? PurposePerson { get; set; }
    }

    public class TrxVisitorCheckinDto : BaseModelDto
    {
        public Guid TrxVisitorId { get; set; }
        public Guid CardId { get; set; }
    }
}