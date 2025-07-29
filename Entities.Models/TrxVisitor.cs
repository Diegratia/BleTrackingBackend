using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class TrxVisitor : IApplicationEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("checked_in_at")]
        public DateTime?CheckedInAt { get; set; }

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

        [ForeignKey(nameof(MaskedArea))]
        [Column("masked_area_id")]
        public Guid? MaskedAreaId { get; set; }

        [Column("parking_id")]
        public Guid? ParkingId { get; set; }

        [Column("visitor_id")]    
        [ForeignKey(nameof(Visitor))]
        public Guid? VisitorId { get; set; } 

        [Required]
        [ForeignKey("ApplicationId")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public FloorplanMaskedArea? MaskedArea { get; set; }
        public MstApplication Application { get; set; }
        public virtual Visitor? Visitor { get; set; }     

    }
}
