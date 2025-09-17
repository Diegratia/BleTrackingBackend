using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
  [Table("card_groups")]
  public class CardGroup : BaseModelOnlyIdWithTime, IApplicationEntity
  {
    [Column("name")]
    public string? Name { get; set; }

    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("access_scope")]
    public AccessScope? AccessScope { get; set; }

    [Column("application_id")]
    public Guid ApplicationId { get; set; }

    [Required]
    [Column("status")]
    public int Status { get; set; } = 1;

    public MstApplication Application { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public ICollection<CardGroupCardAccess?> CardGroupCardAccesses { get; set; } = new List<CardGroupCardAccess?>();

     [NotMapped]
     public ICollection<CardAccess> CardAccesses => CardGroupCardAccesses.Select(cga => cga.CardAccess).ToList();
  }
}