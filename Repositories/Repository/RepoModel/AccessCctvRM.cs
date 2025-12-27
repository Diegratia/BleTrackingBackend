using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Repositories.Repository.RepoModel
{
    public class MinimalAccessCctvRM 
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Rtsp { get; set; }
        public Guid? IntegrationId { get; set; }
    }
}