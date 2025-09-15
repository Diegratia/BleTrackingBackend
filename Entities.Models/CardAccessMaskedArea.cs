using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
  [Table("card_access_masked_areas")]
  public class CardAccessMaskedArea : IApplicationEntity
  {
    [Column("card_access_id")]
    public Guid? CardAccessId { get; set; }
    public CardAccess? CardAccess { get; set; }

    [Column("masked_area_id")]
    public Guid? MaskedAreaId { get; set; }
    public FloorplanMaskedArea? MaskedArea { get; set; }

    [ForeignKey("Application")]
    [Column("application_id")]
    public Guid ApplicationId { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;
    
    }
}