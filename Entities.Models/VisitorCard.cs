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
    public class VisitorCard
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = "";

        [Column("number")]
        public string Number { get; set; } = "";

        [Column("type")]
        public CardType Type { get; set; }

        [Column("qr_code")]
        public string QRCode { get; set; } = "";

        [Column("mac")]
        public string mac { get; set; }

        [Column("checkin_status")]
        public int CheckinStatus { get; set; }

        [Column("enable_status")]
        public int EnableStatus { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("site_id")]
        public Guid SiteId { get; set; }

        [Column("is_member")]
        public int IsMember { get; set; }

        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        public virtual MstApplication Application { get; set; }
        public virtual ICollection<CardRecord> CardRecords { get; set; } = new List<CardRecord>();
    }
}