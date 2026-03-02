using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    [Table("card_card_accesses")]
    public class CardCardAccess : IApplicationEntity
    {
        [Column("card_id")]
        public Guid CardId { get; set; }
        public Card Card { get; set; }

        [Column("card_access_id")]
        public Guid CardAccessId { get; set; }
        public CardAccess CardAccess { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; } = 1;
    
        [ForeignKey("Application")]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }
    }
}