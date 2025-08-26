using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace Helpers.Consumer.DtoHelpers
{
    public class VisitorBlacklistAreaDtoMinimal : BaseModelDto
    {
           public Guid Id { get; set; }

        public Guid VisitorId { get; set; }
        public Guid FloorplanMaskedAreaId { get; set; }
       
        public FloorplanMaskedAreaDtoMinimal? FloorplanMaskedArea { get; set; }
        public VisitorDtoMinimal? Visitor { get; set; }

    }
}