using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;

namespace BusinessLogic.Services.Interface
{
    public interface ITimeBlockService
    {
         Task<TimeBlockDto> CreateAsync(TimeBlockCreateDto dto);
      
    }
}