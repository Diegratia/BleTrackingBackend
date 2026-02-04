using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using DataView;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Interface
{
    public interface IUserGroupService
    {
        Task<UserGroupRead> GetByIdAsync(Guid id);
        Task<IEnumerable<UserGroupRead>> GetAllAsync();
        Task<UserGroupDto> CreateAsync(CreateUserGroupDto dto);
        Task UpdateAsync(Guid id, UpdateUserGroupDto dto);
        Task DeleteAsync(Guid id);
        Task<object> FilterAsync(DataTablesProjectedRequest request, UserGroupFilter filter);
    }
}
