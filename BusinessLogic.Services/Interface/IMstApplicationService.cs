using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstApplicationService
    {
        Task<IEnumerable<MstApplicationDto>> GetAllApplicationsAsync();
        Task<MstApplicationDto> GetApplicationByIdAsync(Guid id);
        Task<MstApplicationDto> CreateApplicationAsync(MstApplicationCreateDto dto);
        Task UpdateApplicationAsync(Guid id, MstApplicationUpdateDto dto);
        Task DeleteApplicationAsync(Guid id);
    }
}