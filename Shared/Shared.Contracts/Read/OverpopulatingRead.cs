using System;

namespace Shared.Contracts.Read
{
    public class OverpopulatingRead : BaseRead
    {
        public string Name { get; set; }
        public string AreaShape { get; set; }
        public string Color { get; set; }
        public string Remarks { get; set; }
        public Guid? FloorplanId { get; set; }
        public string FloorplanName { get; set; }
        public Guid? FloorId { get; set; }
        public string FloorName { get; set; }
        public int? MaxCapacity { get; set; }
        public int IsActive { get; set; }
    }
}
