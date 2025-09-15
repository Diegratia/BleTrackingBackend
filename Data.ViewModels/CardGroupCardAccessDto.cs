using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class CardGroupCardAccessCreateDto : BaseModelDto
    {
        public Guid CardGroupId { get; set; }
        public Guid CardAccessId { get; set; }
    }

    public class CardGroupCardAccessDto : BaseModelDto
    {
        public Guid CardGroupId { get; set; }
        public Guid CardAccessId { get; set; }
    }
}