using System;

namespace Data.ViewModels
{
    public class MstApplicationDto
    {
        public long Generate { get; set; }
        public Guid Id { get; set; }
        public string ApplicationName { get; set; }
        public string OrganizationType { get; set; }
        public string OrganizationAddress { get; set; }
        public string ApplicationType { get; set; }
        public DateTime ApplicationRegistered { get; set; }
        public DateTime ApplicationExpired { get; set; }
        public string HostName { get; set; }
        public string HostPhone { get; set; }
        public string HostEmail { get; set; }
        public string HostAddress { get; set; }
        public string ApplicationCustomName { get; set; }
        public string ApplicationCustomDomain { get; set; }
        public string ApplicationCustomPort { get; set; }
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
        public int? ApplicationStatus { get; set; }
    }

    public class MstApplicationCreateDto
    {
        public string ApplicationName { get; set; }
        public string OrganizationType { get; set; }
        public string OrganizationAddress { get; set; }
        public string ApplicationType { get; set; }
        // public DateTime ApplicationRegistered { get; set; }
        public DateTime ApplicationExpired { get; set; }
        public string HostName { get; set; }
        public string HostPhone { get; set; }
        public string HostEmail { get; set; }
        public string HostAddress { get; set; }
        public string ApplicationCustomName { get; set; }
        public string ApplicationCustomDomain { get; set; }
        public string ApplicationCustomPort { get; set; }
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
    }

    public class MstApplicationUpdateDto
    {
        public string ApplicationName { get; set; }
        public string OrganizationType { get; set; }
        public string OrganizationAddress { get; set; }
        public string ApplicationType { get; set; }
        // public DateTime ApplicationRegistered { get; set; }
        public DateTime ApplicationExpired { get; set; }
        public string HostName { get; set; }
        public string HostPhone { get; set; }
        public string HostEmail { get; set; }
        public string HostAddress { get; set; }
        public string ApplicationCustomName { get; set; }
        public string ApplicationCustomDomain { get; set; }
        public string ApplicationCustomPort { get; set; }
        public string LicenseCode { get; set; }
        public string LicenseType { get; set; }
    }
}