using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Repository.RepoModel
{
    public class MstSecurityRM
    {

    }
    public class MstSecurityLookUpRM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? PersonId { get; set; }
        public string? CardNumber { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? OrganizationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? DistrictName { get; set; }
        public Guid? ApplicationId { get; set; }
    }

}