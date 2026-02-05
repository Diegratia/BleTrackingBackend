using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IUserService
    {
        Task<UserRead> GetByIdAsync(Guid id);
        Task<IEnumerable<UserRead>> GetAllAsync();
        Task<IEnumerable<UserRead>> GetAllIntegrationAsync();
        Task<UserDto> CreateAsync(RegisterDto dto);
        Task UpdateAsync(Guid id, UpdateUserDto dto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, UserFilter filter);
    }
}
