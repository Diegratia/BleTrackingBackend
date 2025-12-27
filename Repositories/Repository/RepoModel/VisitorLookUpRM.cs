using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.RepoModel
{
    public class VisitorLookUpRM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? PersonId { get; set; }
        public string? IdentityId { get; set; }
        public string? IdentityType { get; set; }
        public string? CardNumber { get; set; }
        public string? OrganizationName { get; set; }
        public string? DistrictName { get; set; }
        public string? DepartmentName { get; set; }
    }
}