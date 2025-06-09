using System;
using Helpers.Consumer;

namespace Data.ViewModels
{
    public class MstEngineDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EngineId { get; set; }
        public int Port { get; set; }
        public int? Status { get; set; }
        public int? IsLive { get; set; }
        public DateTime LastLive { get; set; }
        public ServiceStatus ServiceStatus { get; set; }
    }

    public class MstEngineCreateDto
    {
        public string Name { get; set; }
        public string EngineId { get; set; }
        public int Port { get; set; }
        public int IsLive { get; set; }
        public DateTime LastLive { get; set; }
        public ServiceStatus ServiceStatus { get; set; }
    }

    public class MstEngineUpdateDto
    {
        public string Name { get; set; }
    }
}