using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace Helpers.Consumer.DtoHelpers
{
    public class FloorplanMaskedAreaDtoMinimal : BaseModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}