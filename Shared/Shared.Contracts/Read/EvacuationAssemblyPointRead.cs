using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class EvacuationAssemblyPointRead : BaseRead
    {
        public string Name { get; set; } = string.Empty;
        public string? AreaShape { get; set; }
        public string? Color { get; set; }
        public string? Remarks { get; set; }
        public Guid? FloorplanId { get; set; }
        public Guid? FloorId { get; set; }
        public int IsActive { get; set; }

        // Navigation properties
        public string? FloorplanName { get; set; }
        public string? FloorName { get; set; }
        public string? BuildingName { get; set; }
    }
}
