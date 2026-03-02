using Shared.Contracts;

namespace Shared.Contracts.Read
{
    public class CardAccessRead : BaseRead
    {
        public string? Name { get; set; }
        public int? AccessNumber { get; set; }
        public string? Remarks { get; set; }
        public AccessScope? AccessScope { get; set; }
        public Guid ApplicationId { get; set; }
        public int Status { get; set; }
    }
}
