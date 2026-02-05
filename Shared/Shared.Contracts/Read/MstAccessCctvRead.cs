using System;

namespace Shared.Contracts.Read
{
    public class MstAccessCctvRead : BaseRead
    {
        public string Name { get; set; }
        public string Rtsp { get; set; }
        public bool IsAssigned { get; set; }
        public Guid? IntegrationId { get; set; }
        public IntegrationType? IntegrationType { get; set; }
    }
}
