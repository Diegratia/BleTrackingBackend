using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class Card : BaseModelWithTime
    {
        [Column("name")]
        public string? Name { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("type")]
        public CardType? CardType { get; set; }

        [Column("card_number")]
        public string? CardNumber { get; set; }

        [Column("card_barcode")]
        public string? CardBarcode { get; set; }

        [Column("is_multi_site")]
        public bool? IsMultiSite { get; set; }

        [Column("registered_site")]
        public Guid? RegisteredSite { get; set; } // isikan  null jika bisa digunakan disemua site.

        [Column("is_used")]
        public bool? IsUsed { get; set; } = false;

        [Column("last_used_by")]
        public string? LastUsed { get; set; } 

        [Column("status_card")]
        public bool? StatusCard { get; set; } = true;

    }
}



