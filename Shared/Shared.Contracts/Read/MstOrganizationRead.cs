using System;

namespace Shared.Contracts.Read
{
    public class MstOrganizationRead : BaseRead
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? OrganizationHost { get; set; }
    }
}
