// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations.Schema;
// using System.Linq;
// using System.Threading.Tasks;
// using Helpers.Consumer;

// namespace Entities.Models
// {
//     [Table("card_groups")]
//     public class CardGroup : BaseModelOnlyIdWithTime
//     {
//         [Column("name")]
//         public string Name { get; set; }

//         [Column("remarks")]
//         public string Remarks { get; set; }

//         [Column("application_id")]
//         public Guid ApplicationId { get; set; }
//         public virtual MstApplication Application { get; set; }
//         public ICollection<Card> Card { get; set; } = new List<Card>();
//         public ICollection<CardGroupCardAccess> CardGroupCardAccesses { get; set; } = new List<CardGroupCardAccess>();
//     }

// }