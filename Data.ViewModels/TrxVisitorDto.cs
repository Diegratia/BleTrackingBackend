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
        public BigInteger? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; } 
        public string? VisitorCode { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public string? SiteId { get; set; } 
        public string? ParkingId { get; set; } 
        public Guid? VisitorId { get; set; }
    }

    public class TrxVisitorCreateDto : BaseModelDto
    {
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
        public BigInteger? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; } 
        public string? VisitorCode { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public string? SiteId { get; set; } 
        public string? ParkingId { get; set; } 
        public Guid? VisitorId { get; set; }
    }
    
      public class TrxVisitorUpdateDto : BaseModelDto
    {
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
        public BigInteger? VisitorGroupCode { get; set; }
        public string? VisitorNumber { get; set; } 
        public string? VisitorCode { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public string? Remarks { get; set; }
        public string? SiteId { get; set; } 
        public string? ParkingId { get; set; } 
        public Guid? VisitorId { get; set; }
    }
}