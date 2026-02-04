using System;

namespace Shared.Contracts.Read
{
    public class UserGroupRead : BaseRead
    {
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
    }
}
