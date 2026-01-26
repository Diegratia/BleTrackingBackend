using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Interface
{
    public interface IPatrolCaseService
    {
        Task<object> FilterAsync(
            DataTablesRequest request,
            PatrolCaseFilter filter
        );
        
    }
}