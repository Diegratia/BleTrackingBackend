using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;

namespace BusinessLogic.Services.Interface
{
    public interface IMstBleReaderService
    {
        Task<MstBleReaderDto> GetByIdAsync(Guid id);
        Task<IEnumerable<MstBleReaderDto>> GetAllAsync();
        Task<MstBleReaderDto> CreateAsync(MstBleReaderCreateDto createDto);
        Task UpdateAsync(Guid id, MstBleReaderUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}