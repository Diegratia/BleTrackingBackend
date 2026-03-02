using System;

namespace Shared.Contracts.Read
{
    public class MstDistrictRead : BaseRead
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? DistrictHost { get; set; }
    }
}
