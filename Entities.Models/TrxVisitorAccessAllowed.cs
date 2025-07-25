using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Entities.Models
{
    public class TrxVisitorAccessAllowed : BaseModelOnlyId
    {
        public Guid VisitorId { get; set; }
        public Guid MaskedAreaId { get; set; }
        public int? AllowTime { get; set; } 
    }
}