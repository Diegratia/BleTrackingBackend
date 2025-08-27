using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Helpers.Consumer.DtoHelpers.MinimalDto
{
    public class VisitorDtoMinimal : BaseModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}