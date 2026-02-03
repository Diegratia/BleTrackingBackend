using System;

namespace Shared.Contracts.Read
{
    public class MstBleReaderRead : BaseRead
    {
        public Guid BrandId { get; set; }
        public string? Name { get; set; }
        public string? Ip { get; set; }
        public string? Gmac { get; set; }
        public bool? IsAssigned { get; set; }
        public string? ReaderType { get; set; }
        public MstBrandRead? Brand { get; set; }
    }
}
