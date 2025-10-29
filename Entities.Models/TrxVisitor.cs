using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;

namespace Entities.Models
{

  [Index(nameof(VisitorId))]
  [Index(nameof(Status))]
  [Index(nameof(VisitorId), nameof(Status))]
  [Index(nameof(VisitorPeriodStart))]
  [Index(nameof(VisitorId), nameof(VisitorPeriodStart), nameof(VisitorPeriodEnd))]
  public class TrxVisitor : BaseModelWithTime, IApplicationEntity
  {
    [Column("checked_in_at")]
    public DateTime? CheckedInAt { get; set; }

    [Column("checked_out_at")]
    public DateTime? CheckedOutAt { get; set; }

    [Column("deny_at")]
    public DateTime? DenyAt { get; set; }

    [Column("block_at")]
    public DateTime? BlockAt { get; set; }

    [Column("unblock_at")]
    public DateTime? UnblockAt { get; set; }

    [StringLength(255)]
    [Column("checkin_by")]
    public string? CheckinBy { get; set; }

    [StringLength(255)]
    [Column("checkout_by")]
    public string? CheckoutBy { get; set; }

    [StringLength(255)]
    [Column("deny_by")]
    public string? DenyBy { get; set; }

    [StringLength(255)]
    [Column("deny_reason")]
    public string? DenyReason { get; set; }

    [StringLength(255)]
    [Column("block_by")]
    public string? BlockBy { get; set; }

    [StringLength(255)]
    [Column("block_reason")]
    public string? BlockReason { get; set; }

    [Required]
    [Column("status")]
    public VisitorStatus? Status { get; set; }

    [StringLength(255)]
    [Column("visitor_status")]
    public VisitorActiveStatus? VisitorActiveStatus { get; set; }

    [Required]
    [Column("invitation_created_at")]
    public DateTime? InvitationCreatedAt { get; set; }

    [Column("visitor_group_code")]
    public long? VisitorGroupCode { get; set; }

    [Column("visitor_number")]
    public string? VisitorNumber { get; set; }

    [Column("visitor_code")]
    public string? VisitorCode { get; set; }

    [Column("vehicle_plate_number")]
    public string? VehiclePlateNumber { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("visitor_period_start")]
    public DateTime? VisitorPeriodStart { get; set; }

    [Column("visitor_period_end")]
    public DateTime? VisitorPeriodEnd { get; set; }

    [Column("is_invitation_accepted")]
    public bool? IsInvitationAccepted { get; set; }

    [Column("invitation_code")]
    public string? InvitationCode { get; set; }

    [Column("invitation_token_expired_at")]
    public DateTime? InvitationTokenExpiredAt { get; set; }

    [ForeignKey(nameof(MaskedArea))]
    [Column("masked_area_id")]
    public Guid? MaskedAreaId { get; set; }

    [Column("parking_id")]
    public Guid? ParkingId { get; set; }

    [Column("visitor_id")]
    [ForeignKey(nameof(Visitor))]
    public Guid? VisitorId { get; set; }

    [Column("purpose_person_id")]
    [ForeignKey(nameof(Member))]
    public Guid? PurposePerson { get; set; }

    [Column("member_identity")]
    public string? MemberIdentity { get; set; }

    [Required]
    [ForeignKey("ApplicationId")]
    [Column("application_id")]
    public Guid ApplicationId { get; set; }
    public int TrxStatus { get; set; }
    
    [Column("is_member")]
    public int? IsMember { get; set; }

    [Column("person_type")]
    public PersonType? PersonType { get; set; }
  
    [Column("agenda")]
    public string? Agenda { get; set; }

    [Column("extended_visitor_time")]
    public int? ExtendedVisitorTime { get; set; }

    public FloorplanMaskedArea? MaskedArea { get; set; }
    public MstApplication Application { get; set; }
    public Visitor? Visitor { get; set; }
    public MstMember? Member { get; set; }

  }
}
