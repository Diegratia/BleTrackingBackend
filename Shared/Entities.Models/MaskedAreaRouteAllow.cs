using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MaskedAreaRouteAllow : BaseModelOnlyId
    {
        public Guid MaskedAreaId { get; set; }
        public Guid? AllowAreaId { get; set; } 
        public int? AllowTime { get; set; }
    }
}