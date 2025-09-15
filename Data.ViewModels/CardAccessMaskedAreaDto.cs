using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class CardAccessMaskedAreaCreateDto : BaseModelDto
    {
        public Guid? CardAccessId { get; set; }
        public Guid? MaskedAreaId { get; set; }
    }

    public class CardAccessMaskedAreaDto : BaseModelDto
    {
      public Guid? CardAccessId { get; set; }
      public Guid? MaskedAreaId { get; set; }
    }
}