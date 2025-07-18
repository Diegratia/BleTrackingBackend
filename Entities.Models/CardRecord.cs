using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;
using System.Diagnostics.CodeAnalysis;

namespace Entities.Models
{
    public class CardRecord
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("visitor_name")]
        public string VisitorName { get; set; }

        [ForeignKey(nameof(VisitorCard))]
        [Column("card_id")]
        public Guid CardId { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Visitor))]
        [Column("visitor_id")]
        public Guid? VisitorId { get; set; }

        [AllowNull]
        [ForeignKey(nameof(Member))]
        [Column("member_id")]
        public Guid? MemberId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("checkin_at")]
        public DateTime? CheckinAt { get; set; }

        [Column("checkout_at")]
        public DateTime? CheckoutAt { get; set; }

        [Column("checkin_by")]
        public string CheckinBy { get; set; } = "";

        [Column("checkout_by")]
        public string CheckoutBy { get; set; } = "";

        [Column("checkout_site_id")]
        public Guid? CheckoutSiteId { get; set; }

        [Column("checkin_site_id")]
        public Guid? CheckinSiteId { get; set; }

        [Column("visitor_type")]
        public VisitorType VisitorType { get; set; }

        public virtual Visitor Visitor { get; set; }
        public virtual MstMember Member { get; set; }
        public virtual VisitorCard VisitorCard { get; set; }
         
   
        
    }
}