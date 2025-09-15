// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations.Schema;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Entities.Models
// {
//     [Table("card_group_card_accesses")]
//     public class CardGroupCardAccess
//     {
//         [Column("card_group_id")]
//         public Guid CardGroupId { get; set; }
//         public CardGroup CardGroup { get; set; }

//         [Column("card_access_id")]
//         public Guid CardAccessId { get; set; }
//         public CardAccess CardAccess { get; set; }
    
//         [ForeignKey("Application")]
//         [Column("application_id")]
//         public Guid ApplicationId { get; set; }
//     }
// }