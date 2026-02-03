using System;
using System.Text.Json.Serialization;

namespace Shared.Contracts.Read
{
    public class MstBuildingRead : BaseRead
    {
        public string Name { get; set; }
        public string Image { get; set; }
    }
}
