using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Contracts.Read
{
    public class MstSecurityRead
    {
        public Guid Id { get; set; }
        public string? PersonId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? IdentityId { get; set; }
        public string? CardNumber { get; set; }
        public string? BleCardNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? FaceImage { get; set; }
        public int? UploadFr { get; set; }
        public string? UploadFrError { get; set; }
        public DateOnly? BirthDate { get; set; }
        public DateOnly? JoinDate { get; set; }
        public DateOnly? ExitDate { get; set; }
        public string? HeadMember1 { get; set; }
        public string? HeadMember2 { get; set; }
        public bool? IsBlacklist { get; set; }
        public DateTime? BlacklistAt { get; set; }
        public string? BlacklistReason { get; set; }
        public Guid ApplicationId { get; set; }
        public string? StatusEmployee { get; set; }
        public int Status { get; set; }
        public bool? IsHead { get; set; }
        public OrganizationRead? Organization { get; set; }
        public DepartmentRead? Department { get; set; }
        public DistrictRead? District { get; set; }
    }

    public class OrganizationRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class DepartmentRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class DistrictRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class MstSecurityLookUpRead
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
        public string? Email { get; set; }
        public bool? IsHead { get; set; }
    }

     public class SecurityListRead
        {
            public Guid? Id { get; set; }
            public string? Name { get; set; }
            public string? CardNumber { get; set; }
            public string? IdentityId { get; set; }
            public string? OrganizationName { get; set; }
            public string? DepartmentName { get; set; }
            public string? DistrictName { get; set; }
        }

}