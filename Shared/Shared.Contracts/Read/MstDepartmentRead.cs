using System;

namespace Shared.Contracts.Read
{
    public class MstDepartmentRead : BaseRead
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? DepartmentHost { get; set; }
    }
}
