using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
using Helpers.Consumer.DtoHelpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data.ViewModels.Dto.Helpers.MinimalDto
{
    public class TrxVisitorDtoz : BaseModelDto
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
        public FloorplanMaskedAreaDto? Maskedarea { get; set; }
        public VisitorDto? Visitor { get; set; }
        public MstMemberDto? Member { get; set; }
    }
}